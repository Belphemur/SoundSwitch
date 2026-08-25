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

#if NIGHTLY
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using NuGet.Versioning;

using Sentry;

using Serilog;

using SoundSwitch.Framework.Updater.Releases;
using SoundSwitch.Framework.Updater.Releases.Models;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.Updater;

/// <summary>
/// Checker for the nightly update channel. A nightly install behaves like a beta-track
/// install of its base version: whenever a stable or pre-release on the release train is
/// newer than that base, it is offered (moving the user back onto the release train).
/// Otherwise the newest nightly artifact newer than the running application is offered,
/// its integrity verified through its SHA-512 checksum.
/// </summary>
public class NightlyUpdateChecker(Uri feedUrl) : IUpdateChecker
{
    private const string ReleaseTrainUrl = "https://api.github.com/repos/Belphemur/SoundSwitch/releases";

    public bool Beta { get; set; }

    public event EventHandler<UpdateChecker.NewReleaseEvent> UpdateAvailable;

    public async Task CheckForUpdate(CancellationToken token)
    {
        using var httpClient = new HttpClient(new SentryHttpMessageHandler());
        httpClient.DefaultRequestHeaders.UserAgent.Add(ApplicationInfo.ProductValue);
        httpClient.DefaultRequestHeaders.UserAgent.Add(ApplicationInfo.CommentValue);
        httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

        // The release train takes priority: a nightly can always be upgraded back onto
        // stable/beta. Pre-releases are always included (beta-track semantics). If the
        // release train cannot be reached, fall through to the nightly feed instead of
        // blocking nightly updates entirely.
        var baseVersion = GetBaseVersion(Application.ProductVersion);
        if (baseVersion != null)
        {
            try
            {
                var releases = await httpClient.GetFromJsonAsync(ReleaseTrainUrl, GithubReleasesJsonContext.Default.ReleaseArray, token);
                var trainRelease = BuildReleaseTrainUpdate(releases, baseVersion);
                if (trainRelease != null)
                {
                    UpdateAvailable?.Invoke(this, new UpdateChecker.NewReleaseEvent(trainRelease));
                    return;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Warning(ex, "Unable to query the release train; falling back to the nightly feed.");
            }
        }

        var feed = await httpClient.GetFromJsonAsync(feedUrl, NightlyFeedJsonContext.Default.NightlyFeed, token);
        if (feed == null)
        {
            return;
        }

        var appVersion = NightlyVersion.Parse(Application.ProductVersion);
        var artifact = SelectArtifact(feed, appVersion);
        if (artifact == null)
        {
            return;
        }

        var version = NightlyVersion.Parse(artifact.Version);
        var installer = new Asset
        {
            Name = Path.GetFileName(new Uri(artifact.Url).AbsolutePath),
            BrowserDownloadUrl = artifact.Url
        };
        var release = new AppRelease(ToSemanticVersion(version), installer,
            string.Format(UpdateDownloadStrings.nightlyReleaseName, version));
        if (artifact.Changelog != null)
        {
            release.Changelog.AddRange(artifact.Changelog);
        }

        release.ExpectedSha512 = artifact.Sha512;
        UpdateAvailable?.Invoke(this, new UpdateChecker.NewReleaseEvent(release));
    }

    /// <summary>
    /// Selects the artifact to offer. Returns the highest version strictly newer than the
    /// running application, skipping any artifact that lacks a SHA-512 checksum.
    /// </summary>
    internal static NightlyArtifact SelectArtifact(NightlyFeed feed, NightlyVersion appVersion)
    {
        NightlyArtifact best = null;
        NightlyVersion bestVersion = default;

        foreach (var artifact in feed?.Artifacts ?? new List<NightlyArtifact>())
        {
            if (string.IsNullOrWhiteSpace(artifact.Sha512))
            {
                // Expected for artifacts published before checksums existed; skip quietly.
                Log.Information("Nightly artifact {Version} is missing its sha512 checksum; it will not be offered.", artifact.Version);
                continue;
            }

            if (!NightlyVersion.TryParse(artifact.Version, out var version))
            {
                continue;
            }

            if (version <= appVersion)
            {
                continue;
            }

            if (best == null || version > bestVersion)
            {
                best = artifact;
                bestVersion = version;
            }
        }

        return best;
    }

    /// <summary>
    /// The nightly's base version: the first three numeric parts of the running
    /// application version (e.g. 7.2.1.341149 → 7.2.1). Null when unparseable.
    /// </summary>
    internal static SemanticVersion GetBaseVersion(string appVersion)
    {
        if (string.IsNullOrWhiteSpace(appVersion))
        {
            return null;
        }

        var parts = appVersion.Split('.');
        if (parts.Length < 3)
        {
            return null;
        }

        if (!int.TryParse(parts[0], out var major) ||
            !int.TryParse(parts[1], out var minor) ||
            !int.TryParse(parts[2], out var build))
        {
            return null;
        }

        return new SemanticVersion(major, minor, build);
    }

    /// <summary>
    /// Selects the release-train update to offer a nightly install: the highest stable or
    /// pre-release strictly newer than <paramref name="baseVersion"/>, pre-releases always
    /// included (beta-track semantics), with the same arch-suffix asset preference as
    /// <see cref="UpdateChecker"/>. Returns null when no such release exists.
    /// </summary>
    internal static AppRelease BuildReleaseTrainUpdate(IEnumerable<Release> releases, SemanticVersion baseVersion)
    {
        SemanticVersion best = null;
        Release bestRelease = null;
        foreach (var release in releases ?? Array.Empty<Release>())
        {
            // Same normalization as UpdateChecker.ProcessAndNotifyRelease for beta tags.
            var tagName = release.TagName;
            if (release.Prerelease && tagName != null && !tagName.Contains("-beta."))
            {
                tagName += "-beta.1";
            }

            if (tagName == null || !SemanticVersion.TryParse(tagName.Substring(1), out var version))
            {
                continue;
            }

            // Same major.minor.patch as the nightly's base still counts: installing that
            // stable/beta moves the user back onto the release train. Only strictly older
            // bases are ineligible.
            var eligible = version.Major > baseVersion.Major
                           || (version.Major == baseVersion.Major && version.Minor > baseVersion.Minor)
                           || (version.Major == baseVersion.Major && version.Minor == baseVersion.Minor
                               && version.Patch >= baseVersion.Patch);
            if (!eligible)
            {
                continue;
            }

            if (best == null || version > best)
            {
                best = version;
                bestRelease = release;
            }
        }

        if (bestRelease == null)
        {
            return null;
        }

        var archSuffix = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "_arm64" : "_x64";
        var installer = bestRelease.Assets.FirstOrDefault(a => a.Name.Contains(archSuffix) && a.Name.EndsWith(".exe"))
                        ?? bestRelease.Assets.FirstOrDefault(a => a.Name.EndsWith(".exe")
                                                                   && !a.Name.Contains("_arm64")
                                                                   && !a.Name.Contains("_x64"));
        if (installer == null)
        {
            return null;
        }

        return new AppRelease(best, installer, bestRelease.Name);
        // ExpectedSha512 stays null: release installers are Authenticode-signed.
    }

    internal static SemanticVersion ToSemanticVersion(NightlyVersion version)
    {
        // Preserve nightly ordering across revision boundaries: keep major.minor.build as
        // the release version and encode the revision as a numeric prerelease identifier
        // (e.g. 7.2.1-nightly.200000). Numeric prerelease identifiers compare by value,
        // so ordering always matches the feed (no %100_000 wrap-around), and every
        // nightly sorts below its own base release — consistent with the rule that a
        // release-train update wins over nightly artifacts.
        return SemanticVersion.Parse($"{version.Major}.{version.Minor}.{version.Build}-nightly.{version.Revision}");
    }
}
#endif
