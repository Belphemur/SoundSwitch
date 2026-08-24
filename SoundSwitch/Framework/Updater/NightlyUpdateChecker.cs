#if NIGHTLY
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using NuGet.Versioning;

using Sentry;

using Serilog;

using SoundSwitch.Framework.Updater.Releases;
using SoundSwitch.Framework.Updater.Releases.Models;

namespace SoundSwitch.Framework.Updater;

/// <summary>
/// Checker for the nightly update channel. It reads the nightly feed, selects the
/// highest artifact newer than the running application, and maps it to an
/// <see cref="AppRelease"/> whose integrity is verified through its SHA-512 checksum.
/// </summary>
public class NightlyUpdateChecker(Uri feedUrl) : IUpdateChecker
{
    public bool Beta { get; set; }

    public event EventHandler<UpdateChecker.NewReleaseEvent> UpdateAvailable;

    public async Task CheckForUpdate(CancellationToken token)
    {
        using var httpClient = new HttpClient(new SentryHttpMessageHandler());
        httpClient.DefaultRequestHeaders.UserAgent.Add(ApplicationInfo.ProductValue);
        httpClient.DefaultRequestHeaders.UserAgent.Add(ApplicationInfo.CommentValue);
        httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
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
        var release = new AppRelease(ToSemanticVersion(version), installer, $"SoundSwitch Nightly {version}");
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
                Log.Error("Nightly artifact {Version} is missing its sha512 checksum; it will not be offered.", artifact.Version);
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

    private static SemanticVersion ToSemanticVersion(NightlyVersion version)
    {
        var patch = version.Revision % 100_000;
        return new SemanticVersion(version.Major, version.Minor, patch);
    }
}
#endif
