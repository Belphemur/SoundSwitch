/********************************************************************
* Copyright (C) 2015-2017 Antoine Aflalo
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either version 2
* of the License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
********************************************************************/

using System;
using System.IO;
using System.Threading;

using Newtonsoft.Json;

using Serilog;

namespace SoundSwitch.Framework.Configuration;

public interface IConfiguration
{
    /// <summary>
    /// Where is the configuration saved
    /// Any change to this will be discarded at the next loading of the configuration
    /// </summary>
    string FileLocation { get; set; }

    void Save();

    /// <summary>
    /// Migrate configuration to a new schema
    /// </summary>
    bool Migrate();
}

public static class ConfigurationManager
{
    private static readonly string SRoot = ApplicationPath.Default;

    static ConfigurationManager()
    {
        if (Directory.Exists(SRoot))
            return;

        Directory.CreateDirectory(SRoot);
    }

    /// <summary>
    /// Load a ConfigurationManager from its file
    /// </summary>
    /// <returns>The loaded configuration if the file exists, else a new instance of the configuration</returns>
    public static T LoadConfiguration<T>() where T : IConfiguration, new()
    {
        var filePath = GetFilePath<T>();
        T obj;
        if (!File.Exists(filePath))
        {
            obj = new T();
        }
        else
        {
            var serializer = new JsonSerializer {NullValueHandling = NullValueHandling.Ignore};
            using (var fileOpen = File.OpenRead(filePath))
            using (var reader = new JsonTextReader(new StreamReader(fileOpen)))
            {
                obj = serializer.Deserialize<T>(reader);
            }

            if (obj == null)
            {
                Log.Warning("Can't deserialize the config file {type}: {filePath}", typeof(T), filePath);
                obj = new T();
            }
        }

        obj.FileLocation = filePath;
        if (obj.Migrate())
        {
            obj.Save();
        }

        return obj;
    }

    private static string GetFilePath<T>() where T : IConfiguration, new()
    {
        var filePath = Path.Combine(SRoot, typeof(T).Name + ".json");
        return filePath;
    }

    /// <summary>
    /// Serializes all configuration saves: writers from the UI thread, the process/foreground
    /// monitors and the self-heal path must not interleave writes to the same file.
    /// </summary>
    private static readonly object SaveLock = new();

    /// <summary>
    /// How many times a save is attempted before giving up, to ride out short-lived
    /// locks taken by other processes (second instance, backup/AV scanners) on the files.
    /// </summary>
    private const int SaveMaxAttempts = 3;

    /// <summary>
    /// Delay between two save attempts, letting the locking process release the files.
    /// </summary>
    private static readonly TimeSpan SaveRetryDelay = TimeSpan.FromMilliseconds(150);

    /// <summary>
    /// Save the configuration in a json file.
    /// </summary>
    /// <param name="configuration">configuration object to save</param>
    public static void SaveConfiguration<T>(T configuration) where T : IConfiguration, new()
    {
        SaveConfigurationToPath(configuration, GetFilePath<T>());
    }

    /// <summary>
    /// Serializes the configuration to a temporary file next to <paramref name="filePath"/>,
    /// then atomically replaces it, so readers only ever see a complete JSON document.
    /// Transient locks held by other processes on the files are retried briefly; if the save
    /// still fails because of such a lock, the previous configuration on disk is kept and the
    /// error is logged. Other failures (e.g. a full disk or denied access) propagate to the
    /// caller.
    /// </summary>
    /// <param name="onTransientLockRetried">Optional hook invoked after an attempt failed on a
    /// transient lock and the retry was scheduled; used by tests to synchronize on the failure.</param>
    internal static void SaveConfigurationToPath<T>(T configuration, string filePath, Action<Exception> onTransientLockRetried = null) where T : IConfiguration, new()
    {
        lock (SaveLock)
        {
            // Include the process id so two SoundSwitch instances never serialize on
            // the same temporary file (SaveLock only covers writers within one process).
            var tempPath = filePath + "." + Environment.ProcessId + ".tmp";
            try
            {
                var serializer = new JsonSerializer {NullValueHandling = NullValueHandling.Ignore};
                for (var attempt = 1; ; attempt++)
                {
                    try
                    {
                        using (var streamWriter = new StreamWriter(tempPath, append: false))
                        using (var writer = new JsonTextWriter(streamWriter))
                        {
                            serializer.Serialize(writer, configuration);
                        }

                        File.Move(tempPath, filePath, overwrite: true);
                        return;
                    }
                    catch (Exception ex) when (IsTransientFileLockError(ex, filePath) && attempt < SaveMaxAttempts)
                    {
                        Log.Warning(ex,
                            "Attempt {attempt}/{maxAttempts} to save configuration to {filePath} failed, retrying",
                            attempt, SaveMaxAttempts, filePath);
                        onTransientLockRetried?.Invoke(ex);
                        Thread.Sleep(SaveRetryDelay);
                    }
                }
            }
            catch (Exception ex) when (IsTransientFileLockError(ex, filePath))
            {
                Log.Error(ex,
                    "Unable to save configuration to {filePath} after {maxAttempts} attempts: {message}. The previous configuration on disk is kept",
                    filePath, SaveMaxAttempts, ex.Message);
            }
            finally
            {
                try
                {
                    // Best effort: no-op if the temp file has been moved to the destination already.
                    File.Delete(tempPath);
                }
                catch (Exception ex)
                {
                    // A leaked temporary file is harmless: the next save overwrites or deletes it.
                    Log.Debug(ex, "Could not delete temporary configuration save file {tempPath}", tempPath);
                }
            }
        }
    }

    /// <summary>
    /// Is this error caused by another process holding the files, worth retrying?
    /// </summary>
    /// <remarks>
    /// Locks surface in several shapes: sharing, lock, delete-pending or user-mapped-file
    /// violations while opening or replacing a file, and plain access denied when replacing
    /// a destination that another process holds open without any share flags. Other
    /// failures, e.g. a full disk or denied permissions, are permanent and must not be
    /// retried or swallowed.
    /// </remarks>
    private static bool IsTransientFileLockError(Exception exception, string destinationPath)
    {
        const int ErrorSharingViolation = 32;
        const int ErrorLockViolation = 33;
        const int ErrorDeletePending = 303;
        const int ErrorUserMappedFile = 1224;

        switch (exception)
        {
            case IOException ioException when (ioException.HResult & 0xFFFF) is ErrorSharingViolation
                or ErrorLockViolation or ErrorDeletePending or ErrorUserMappedFile:
                return true;
            case UnauthorizedAccessException:
                try
                {
                    return File.Exists(destinationPath) &&
                           !File.GetAttributes(destinationPath).HasFlag(FileAttributes.ReadOnly);
                }
                catch (FileNotFoundException)
                {
                    // The destination vanished; that is not a permission verdict, let the retry decide.
                    return false;
                }
            default:
                return false;
        }
    }
}
