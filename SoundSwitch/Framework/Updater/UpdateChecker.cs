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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace SoundSwitch.Framework.Updater
{
    public class UpdateChecker
    {
        private static readonly string UserAgent =
            $"Mozilla/5.0 (compatible; {Environment.OSVersion.Platform} {Environment.OSVersion.VersionString}; {Application.ProductName}/{Application.ProductVersion};)";

        private static readonly Version AppVersion = new Version(Application.ProductVersion);

        private readonly Uri _releaseUrl;
        private readonly WebClient _webClient = new WebClient();
        public EventHandler<NewReleaseEvent> UpdateAvailable;
        public bool Beta { get; set; }

        public UpdateChecker(Uri releaseUrl) : this(releaseUrl, false)
        {
        }

        public UpdateChecker(Uri releaseUrl, bool checkBeta)
        {
            _releaseUrl = releaseUrl;
            _webClient.DownloadStringCompleted += DownloadStringCompleted;
            Beta = checkBeta;
        }

        private void DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                AppLogger.Log.Error("Exception while getting release ", e.Error);
                return;
            }

            var serverRelease = JsonConvert.DeserializeObject<List<GitHubRelease>>(e.Result);
            if (!serverRelease.Any(ProcessRelease))
            {
                AppLogger.Log.Info("No new Version found ", string.Join("\n", serverRelease));
            }
        }

        private bool ProcessRelease(GitHubRelease serverRelease)
        {
            using (AppLogger.Log.InfoCall())
            {
                AppLogger.Log.Info("Checking version: ", serverRelease);
                if (serverRelease.prerelease && !Beta)
                {
                    AppLogger.Log.Info("Pre-release and not in Beta Mode.");
                    return false;
                }

                var version = new Version(serverRelease.tag_name.Substring(1));
                var changelog = Regex.Split(serverRelease.body, "\r\n|\r|\n");
                try
                {
                    if (version > AppVersion)
                    {
                        var installer = serverRelease.assets.First(asset => asset.name.EndsWith(".exe"));
                        var release = new Release(version, installer, serverRelease.name);
                        release.Changelog.AddRange(changelog);
                        UpdateAvailable?.Invoke(this, new NewReleaseEvent(release));
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.Log.Error("Exception while getting release ", ex);
                }
                return false;
            }
        }

        /// <summary>
        /// Check for update
        /// </summary>
        public void CheckForUpdate()
        {
            _webClient.Headers.Add("User-Agent", UserAgent);
            Task.Factory.StartNew(() => _webClient.DownloadStringAsync(_releaseUrl));
        }

        public class NewReleaseEvent : EventArgs
        {
            public Release Release { get; private set; }

            public NewReleaseEvent(Release release)
            {
                Release = release;
            }
        }
    }
}