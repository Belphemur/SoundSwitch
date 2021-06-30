/********************************************************************
* Copyright (C) 2015-2017 Antoine Aflalo
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of the Lesser GNU General Public License
* as published by the Free Software Foundation; either version 3
* of the License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
********************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;

namespace SoundSwitch.Framework.Updater.Installer
{
    public class WebFile
    {
        private static readonly string UserAgent =
            $"Mozilla/5.0 (compatible; {Environment.OSVersion.Platform} {Environment.OSVersion.VersionString}; {Application.ProductName}/{Application.ProductVersion};)";

        public WebFile(Uri fileUri) : this(fileUri, DefaultFilePath(fileUri))
        {
        }

        public WebFile(Uri fileUri, string filePath)
        {
            if (fileUri == null || filePath == null)
            {
                throw new ArgumentNullException();
            }

            FileUri = fileUri;
            FilePath = filePath;
        }


        public Uri FileUri { get; }
        public string FilePath { get; }
        public bool DownloadStarted { get; private set; }

        private static string DefaultFilePath(Uri fileUri)
        {
            var fi = new FileInfo(fileUri.AbsolutePath);
            return Path.Combine(Path.GetTempPath(), fi.Name);
        }

        public event EventHandler<EventArgs> Downloaded;
        public event EventHandler<EventArgs> Cancelled;
        public event EventHandler<DownloadFailEvent> DownloadFailed;
        public event EventHandler<DownloadProgress> DownloadProgress;

        private readonly CancellationTokenSource _cancellationTokenSource = new();


        public bool Exists()
        {
            return File.Exists(FilePath);
        }

        public void Delete()
        {
            File.Delete(FilePath);
        }

        public Process Start(string args = null)
        {
            if (!Exists())
            {
                throw new InvalidOperationException("The file to be run doesn't exists");
            }

            return args != null ? Process.Start(FilePath, args) : Process.Start(FilePath);
        }

        public void DownloadFile()
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await using (var stream = File.Open(FilePath, FileMode.Create))
                    {
                        DownloadStarted = true;
                        await FileDownloader.DownloadFileAsync(
                            FileUri,
                            stream,
                            _cancellationTokenSource.Token,
                            (downloaded, total) => { DownloadProgress?.Invoke(this, new DownloadProgress(downloaded, total)); }
                        );
                    }

                    if (Exists())
                    {
                        Downloaded?.Invoke(this, new EventArgs());
                    }
                }
                catch (OperationCanceledException)
                {
                    if (Exists())
                    {
                        Delete();
                    }

                    Cancelled?.Invoke(this, new EventArgs());
                }
                catch (Exception e)
                {
                    Log.Error(e, "Problem downloading file {file}:{url}", FilePath, FileUri);
                    DownloadFailed?.Invoke(this, new DownloadFailEvent(e));
                }
            });
        }

        public void CancelDownload()
        {
            _cancellationTokenSource.Cancel();
        }

        public override string ToString()
        {
            return $"FileUri: {FileUri}, FilePath: {FilePath}";
        }
    }

    public class DownloadProgress : EventArgs
    {
        public long TotalBytes { get; }
        public long DownloadedBytes { get; }
        public double Percentage => (double) DownloadedBytes * 100 / TotalBytes;

        public DownloadProgress(long downloadedBytes, long totalBytes)
        {
            TotalBytes = totalBytes;
            DownloadedBytes = downloadedBytes;
        }
    }

    public class DownloadFailEvent : EventArgs
    {
        public DownloadFailEvent(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; set; }
    }
}