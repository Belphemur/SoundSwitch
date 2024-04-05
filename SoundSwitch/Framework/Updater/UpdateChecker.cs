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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NuGet.Versioning;
using Sentry;
using Serilog;
using SoundSwitch.Framework.Updater.Releases;
using SoundSwitch.Framework.Updater.Releases.Models;

namespace SoundSwitch.Framework.Updater
{
    public partial class UpdateChecker
    {
        private static readonly string UserAgent =
            $"Mozilla/5.0 (compatible; {Environment.OSVersion.Platform} {Environment.OSVersion.VersionString}; {Application.ProductName}/{Application.ProductVersion};)";

        private static readonly SemanticVersion AppVersion = SemanticVersion.Parse(Application.ProductVersion);

        private readonly Uri _releaseUrl;
        public EventHandler<NewReleaseEvent> UpdateAvailable;
        public bool Beta { get; set; }

        public UpdateChecker(Uri releaseUrl, bool checkBeta)
        {
            _releaseUrl = releaseUrl;
            Beta = checkBeta;
        }

        /// <summary>
        /// Process the release, and notify about it if it's newer than the version of the app.
        /// </summary>
        /// <returns>true if the release is newer and has been notified</returns>
        private bool ProcessAndNotifyRelease(Release serverRelease)
        {
            Log.Information("Checking version {version} ", serverRelease);
            if (serverRelease.Prerelease && !Beta)
            {
                Log.Information("Pre-release and not in Beta Mode.");
                return false;
            }

            //Fix for beta releases that aren't using semantic versioning to be sure the comparison works
            if (serverRelease.Prerelease && !serverRelease.TagName.Contains("-beta."))
            {
                serverRelease.TagName += "-beta.1";
            }

            if (!SemanticVersion.TryParse(serverRelease.TagName.Substring(1), out var version))
            {
                Log.Error("Invalid version: {version}", serverRelease.TagName);
                return false;
            }
            try
            {
                if (version > AppVersion)
                {
                    var installer = serverRelease.Assets.SingleOrDefault(asset => asset.Name.EndsWith(".exe"));
                    if (installer == null)
                    {
                        return false;
                    }


                    var changelog = BodyRegex().Split(serverRelease.Body);
                    var release = new AppRelease(version, installer, serverRelease.Name);
                    release.Changelog.AddRange(changelog);
                    UpdateAvailable?.Invoke(this, new NewReleaseEvent(release));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception while getting release");
            }

            return false;
        }

        /// <summary>
        /// Check for update
        /// </summary>
        public async Task CheckForUpdate(CancellationToken token)
        {
            using var httpClient = new HttpClient(new SentryHttpMessageHandler());
            httpClient.DefaultRequestHeaders.UserAgent.Add(ApplicationInfo.ProductValue);
            httpClient.DefaultRequestHeaders.UserAgent.Add(ApplicationInfo.CommentValue);
            httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            var releases = await httpClient.GetFromJsonAsync(_releaseUrl, GithubReleasesJsonContext.Default.ReleaseArray, token);
            foreach (var release in (releases ?? Array.Empty<Release>()).OrderByDescending(release => SemanticVersion.Parse(release.TagName.Substring(1))))
            {
                token.ThrowIfCancellationRequested();
                if (ProcessAndNotifyRelease(release))
                {
                    break;
                }
            }
        }

        public class NewReleaseEvent : EventArgs
        {
            public AppRelease AppRelease { get; }

            public NewReleaseEvent(AppRelease appRelease)
            {
                AppRelease = appRelease;
            }
        }

        [GeneratedRegex("\r\n|\r|\n")]
        private static partial Regex BodyRegex();
    }
}