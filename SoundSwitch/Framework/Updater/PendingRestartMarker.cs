/********************************************************************
 * Copyright (C) 2015 Jeroen Pelgrims
 * Copyright (C) 2015-2024 Antoine Aflalo
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

using Serilog;

namespace SoundSwitch.Framework.Updater;

/// <summary>
/// Sentinel recording "a silent update installer was started, its relaunch may have been
/// missed". Written next to the configuration in <c>{AppData}\SoundSwitch</c>
/// (<see cref="ApplicationPath.Default"/>) as <c>pending-restart</c>, deleted on the next
/// start that finds it.
/// </summary>
/// <remarks>
/// Detection and logging only — a process cannot resurrect itself. The two cases where the
/// Restart Manager's relaunch is missed:
/// <list type="bullet">
/// <item>the closed instance was younger than the 60-second minimum runtime Windows
/// requires of a registered process before it may be restarted (Microsoft,
/// RegisterApplicationRestart remarks) — the instance is closed, nothing restarts it;</item>
/// <item>an OLD-version updater that self-exits right after starting Setup (fixed for all
/// versions from 728bb67b on): the Restart Manager never closed that process, so it never
/// restarts it either.</item>
/// </list>
/// Race safety: the marker is consumed at startup, before the updater can write a new one
/// (the download completes minutes into a session). If the Restart Manager did relaunch the
/// updated executable, that new process still finds the marker its predecessor wrote and
/// consumes it — correct in both cases: the marker only means "an update was in flight",
/// and consuming it can never start a second instance, because only one process can hold
/// the user mutex (Program.cs OldMain).
/// </remarks>
internal static class PendingRestartMarker
{
    public const string FileName = "pending-restart";

    private static readonly ILogger _logger = Log.ForContext(typeof(PendingRestartMarker));

    public static string GetPath(string directory) => Path.Combine(directory, FileName);

    /// <summary>
    /// Record that a silent update installer is about to take over the shutdown. The
    /// content is a timestamp and is never parsed. Best-effort: a failed write only loses
    /// the diagnostic, never the update.
    /// </summary>
    public static void Write(string directory)
    {
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(GetPath(directory), DateTimeOffset.UtcNow.ToString("O"));
            _logger.Information("Recorded a pending restart: the installer will close this instance and may or may not relaunch the updated executable");
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Couldn't write the pending-restart marker to {Directory}", directory);
        }
    }

    /// <summary>
    /// Delete the marker if a previous update left one, and report how this process was
    /// started. Called once at startup, after single-instance ownership is confirmed.
    /// </summary>
    /// <returns>True when a marker was present and consumed.</returns>
    public static bool TryConsume(string directory, bool launchedByRestartManager)
    {
        try
        {
            var path = GetPath(directory);
            if (!File.Exists(path))
            {
                return false;
            }

            File.Delete(path);
            if (launchedByRestartManager)
            {
                _logger.Information("Update finished and the Restart Manager relaunched the updated executable; fallback launch marker consumed");
            }
            else
            {
                _logger.Information("Restart Manager did not relaunch the updated executable (older version self-exited, or instance younger than 60s); fallback launch marker consumed");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Couldn't consume the pending-restart marker in {Directory}", directory);
            return false;
        }
    }
}
