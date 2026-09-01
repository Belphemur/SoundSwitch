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
using System.Security.Cryptography;
using System.Text;

using FluentAssertions;

using NUnit.Framework;

using NuGet.Versioning;

using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.Updater.Releases.Models;
using SoundSwitch.Util;

namespace SoundSwitch.Tests;

/// <summary>
/// Unit tests for the nightly update channel: version ordering, artifact selection,
/// release-train fallback and checksum verification.
/// </summary>
[TestFixture]
public class NightlyUpdateTests
{
    [Test]
    public void NightlyVersion_ShouldParseAllFourParts()
    {
        var version = NightlyVersion.Parse("7.1.0.229925");

        version.Major.Should().Be(7);
        version.Minor.Should().Be(1);
        version.Build.Should().Be(0);
        version.Revision.Should().Be(229925);
    }

    [Test]
    public void NightlyVersion_SameMajorMinorDifferingRevision_ShouldOrderByRevision()
    {
        var lower = NightlyVersion.Parse("7.1.0.100");
        var higher = NightlyVersion.Parse("7.1.0.200");

        (higher > lower).Should().BeTrue();
        (lower < higher).Should().BeTrue();
        (lower == NightlyVersion.Parse("7.1.0.100")).Should().BeTrue();
        (higher == NightlyVersion.Parse("7.1.0.200")).Should().BeTrue();
    }

    [Test]
    public void NightlyVersion_DifferentBuild_ShouldOrderByBuildBeforeRevision()
    {
        var lower = NightlyVersion.Parse("7.1.0.999");
        var higher = NightlyVersion.Parse("7.1.1.0");

        higher.Should().BeGreaterThan(lower);
    }

    private static NightlyArtifact Artifact(string version, string sha512 = "abc")
    {
        return new NightlyArtifact
        {
            Version = version,
            Url = $"https://example.com/nightly/{version}.exe",
            Sha512 = sha512
        };
    }

    [Test]
    public void SelectArtifact_ShouldPickHighestNewerArtifact()
    {
        var feed = new NightlyFeed
        {
            Artifacts = new List<NightlyArtifact>
            {
                Artifact("7.1.0.100"),
                Artifact("7.1.0.300"),
                Artifact("7.1.0.200")
            }
        };
        var appVersion = NightlyVersion.Parse("7.1.0.150");

        var selected = NightlyUpdateChecker.SelectArtifact(feed, appVersion);

        selected.Should().NotBeNull();
        selected.Version.Should().Be("7.1.0.300");
    }

    [Test]
    public void SelectArtifact_ShouldSkipOlderOrEqualArtifacts()
    {
        var feed = new NightlyFeed
        {
            Artifacts = new List<NightlyArtifact>
            {
                Artifact("7.1.0.100"),
                Artifact("7.1.0.150")
            }
        };
        var appVersion = NightlyVersion.Parse("7.1.0.150");

        NightlyUpdateChecker.SelectArtifact(feed, appVersion).Should().BeNull();
    }

    [Test]
    public void SelectArtifact_ShouldSkipArtifactsMissingSha512()
    {
        var feed = new NightlyFeed
        {
            Artifacts = new List<NightlyArtifact>
            {
                Artifact("7.1.0.200", null),
                Artifact("7.1.0.100")
            }
        };
        var appVersion = NightlyVersion.Parse("7.1.0.0");

        var selected = NightlyUpdateChecker.SelectArtifact(feed, appVersion);

        selected.Should().NotBeNull();
        selected.Version.Should().Be("7.1.0.100");
    }

    [Test]
    public void SelectArtifact_AllArtifactsMissingSha512_ShouldReturnNull()
    {
        var feed = new NightlyFeed
        {
            Artifacts = new List<NightlyArtifact>
            {
                Artifact("7.1.0.200", null)
            }
        };
        var appVersion = NightlyVersion.Parse("7.1.0.0");

        NightlyUpdateChecker.SelectArtifact(feed, appVersion).Should().BeNull();
    }

    [Test]
    public void UpdateVerifier_MatchingContent_ShouldPass()
    {
        var content = Encoding.UTF8.GetBytes("nightly installer content");
        var filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".exe");
        File.WriteAllBytes(filePath, content);
        try
        {
            var expected = Convert.ToHexString(SHA512.HashData(content)).ToLowerInvariant();

            var result = UpdateVerifier.Verify(filePath, expected);

            result.UnwrapFailure().Should().BeNull();
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Test]
    public void UpdateVerifier_MismatchedContent_ShouldFail()
    {
        var filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".exe");
        File.WriteAllBytes(filePath, Encoding.UTF8.GetBytes("some content"));
        try
        {
            var result = UpdateVerifier.Verify(filePath, new string('0', 128));

            result.UnwrapFailure().Should().NotBeNull();
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Test]
    public void GetBaseVersion_ShouldReturnFirstThreeParts()
    {
        var baseVersion = NightlyUpdateChecker.GetBaseVersion("7.2.1.341149");

        baseVersion.Should().NotBeNull();
        baseVersion.Major.Should().Be(7);
        baseVersion.Minor.Should().Be(2);
        baseVersion.Patch.Should().Be(1);
    }

    [Test]
    public void GetBaseVersion_InvalidVersions_ShouldReturnNull()
    {
        NightlyUpdateChecker.GetBaseVersion("7.2").Should().BeNull();
        NightlyUpdateChecker.GetBaseVersion("").Should().BeNull();
        NightlyUpdateChecker.GetBaseVersion("a.b.c.d").Should().BeNull();
    }

    [Test]
    public void ToSemanticVersion_ShouldPreserveOrderingAcrossRevisionBoundaries()
    {
        // %100_000 wrap-around would make 200000 (patch 0) compare older than 199999.
        var lower = NightlyUpdateChecker.ToSemanticVersion(NightlyVersion.Parse("7.2.1.199999"));
        var higher = NightlyUpdateChecker.ToSemanticVersion(NightlyVersion.Parse("7.2.1.200000"));

        higher.Should().BeGreaterThan(lower);
        // Every nightly sorts below its own base release, so a release-train update wins.
        lower.Should().BeLessThan(SemanticVersion.Parse("7.2.1"));
    }

    private static Release TrainRelease(string tag, bool prerelease, string installerName = null)
    {
        var release = new Release
        {
            TagName = tag,
            Prerelease = prerelease,
            Name = $"SoundSwitch {tag}",
            Assets = new List<Asset>()
        };
        if (installerName != null)
        {
            release.Assets.Add(new Asset
            {
                Name = installerName,
                BrowserDownloadUrl = $"https://example.com/{installerName}"
            });
        }

        return release;
    }

    [Test]
    public void BuildReleaseTrainUpdate_ShouldPickHighestNewerIncludingPrerelease()
    {
        var releases = new List<Release>
        {
            TrainRelease("v7.2.2", false, "SoundSwitch_v7.2.2_Installer.exe"),
            TrainRelease("v7.3.0-beta.1", true, "SoundSwitch_v7.3.0_Installer.exe")
        };

        var update = NightlyUpdateChecker.BuildReleaseTrainUpdate(releases, SemanticVersion.Parse("7.2.1"));

        update.Should().NotBeNull();
        update.ReleaseVersion.Should().Be(SemanticVersion.Parse("7.3.0-beta.1"));
        // A release-train install is Authenticode-signed: no nightly hash attached.
        update.ExpectedSha512.Should().BeNull();
    }

    [Test]
    public void BuildReleaseTrainUpdate_SameBaseRelease_ShouldNotBeOfferedToNightly()
    {
        // A nightly of base 7.2.1 (e.g. 7.2.1.N) is at least as new as the 7.2.1 release,
        // so a same-base stable release must not be offered (it would be a downgrade).
        var releases = new List<Release>
        {
            TrainRelease("v7.2.1", false, "SoundSwitch_v7.2.1_Installer.exe"),
            TrainRelease("v7.2.1-beta.2", true, "SoundSwitch_v7.2.1_Installer.exe")
        };

        NightlyUpdateChecker.BuildReleaseTrainUpdate(releases, SemanticVersion.Parse("7.2.1"))
            .Should().BeNull();
    }

    [Test]
    public void BuildReleaseTrainUpdate_NightlyWithRevision_ShouldBeLatestOverBaseRelease()
    {
        // Scenario: both 7.3.1 (stable) and 7.3.1.1 (nightly) exist. The running nightly is
        // 7.3.1.1; its base is 7.3.1. The stable release is NOT strictly newer, so it must
        // not be offered — the nightly 7.3.1.1 stays the latest.
        var releases = new List<Release>
        {
            TrainRelease("v7.3.1", false, "SoundSwitch_v7.3.1_Installer.exe")
        };
        var baseVersion = NightlyUpdateChecker.GetBaseVersion("7.3.1.1");

        baseVersion.Should().Be(SemanticVersion.Parse("7.3.1"));
        NightlyUpdateChecker.BuildReleaseTrainUpdate(releases, baseVersion).Should().BeNull();
    }

    [Test]
    public void BuildReleaseTrainUpdate_SameBaseBeta_ShouldNotBeOfferedToNightlyWithHigherRevision()
    {
        // Scenario: both 7.2.5-beta.3 (release-train prerelease) and 7.2.5.10 (nightly,
        // base 7.2.5, revision 10) exist. The running nightly 7.2.5.10 is newer than the
        // same-base beta, so the beta must not be offered — the user stays on the nightly.
        var releases = new List<Release>
        {
            TrainRelease("v7.2.5-beta.3", true, "SoundSwitch_v7.2.5_Installer.exe")
        };
        var baseVersion = NightlyUpdateChecker.GetBaseVersion("7.2.5.10");

        baseVersion.Should().Be(SemanticVersion.Parse("7.2.5"));
        NightlyUpdateChecker.BuildReleaseTrainUpdate(releases, baseVersion).Should().BeNull();
    }

    [Test]
    public void BuildReleaseTrainUpdate_LaterRelease_ShouldStillBeOfferedToNightly()
    {
        // A strictly newer release must still win over the nightly of an older base.
        var releases = new List<Release>
        {
            TrainRelease("v7.3.1", false, "SoundSwitch_v7.3.1_Installer.exe"),
            TrainRelease("v7.3.2", false, "SoundSwitch_v7.3.2_Installer.exe")
        };

        var update = NightlyUpdateChecker.BuildReleaseTrainUpdate(releases, SemanticVersion.Parse("7.3.1"));

        update.Should().NotBeNull();
        update.ReleaseVersion.Should().Be(SemanticVersion.Parse("7.3.2"));
    }

    [Test]
    public void BuildReleaseTrainUpdate_StrictlyOlderOnly_ShouldReturnNull()
    {
        var releases = new List<Release>
        {
            TrainRelease("v7.2.0", false, "SoundSwitch_v7.2.0_Installer.exe"),
            TrainRelease("v7.1.9", false, "SoundSwitch_v7.1.9_Installer.exe")
        };

        NightlyUpdateChecker.BuildReleaseTrainUpdate(releases, SemanticVersion.Parse("7.2.1"))
            .Should().BeNull();
    }

    [Test]
    public void BuildReleaseTrainUpdate_NewerButMissingInstaller_ShouldReturnNull()
    {
        var releases = new List<Release>
        {
            TrainRelease("v7.3.0", false)
        };

        NightlyUpdateChecker.BuildReleaseTrainUpdate(releases, SemanticVersion.Parse("7.2.1"))
            .Should().BeNull();
    }
}
#endif
