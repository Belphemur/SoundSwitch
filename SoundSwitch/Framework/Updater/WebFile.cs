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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundSwitch.Framework.Updater
{
    public class WebFile
    {
        private static readonly string UserAgent =
            $"Mozilla/5.0 (compatible; {Environment.OSVersion.Platform} {Environment.OSVersion.VersionString}; {Application.ProductName}/{Application.ProductVersion};)";

        private readonly WebClient _webClient = new WebClient();

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
            _webClient.DownloadFileCompleted += WebClientOnDownloadFileCompleted;
            _webClient.DownloadProgressChanged +=
                (sender, args) => DownloadProgressChanged?.Invoke(this, args);
        }

        public Uri FileUri { get; }
        public string FilePath { get; }

        private static string DefaultFilePath(Uri fileUri)
        {
            var fi = new FileInfo(fileUri.AbsolutePath);
            return Path.Combine(Path.GetTempPath(), fi.Name);
        }

        public event EventHandler<EventArgs> Downloaded;
        public event EventHandler<EventArgs> Cancelled;
        public event EventHandler<DownloadFailEvent> DownloadFailed;
        public event DownloadProgressChangedEventHandler DownloadProgressChanged;

        private void WebClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            if (asyncCompletedEventArgs.Cancelled)
            {
                if (Exists())
                {
                    Delete();
                }
                Cancelled?.Invoke(this, new EventArgs());
                return;
            }

            if (asyncCompletedEventArgs.Error != null)
            {
                DownloadFailed?.Invoke(this, new DownloadFailEvent(asyncCompletedEventArgs.Error));

                AppLogger.Log.Error("Problem downloading file ", asyncCompletedEventArgs.Error);
                return;
            }


            if (Exists())
            {
                Downloaded?.Invoke(this, new EventArgs());
            }
        }

        protected void PrepareWebClientRequest()
        {
            _webClient.Headers.Add("User-Agent", UserAgent);
        }

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
            PrepareWebClientRequest();
            Task.Factory.StartNew(() => _webClient.DownloadFileAsync(FileUri, FilePath));
        }

        public void CancelDownload()
        {
            _webClient.CancelAsync();
        }

        public override string ToString()
        {
            return $"FileUri: {FileUri}, FilePath: {FilePath}";
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