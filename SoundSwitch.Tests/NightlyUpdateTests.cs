#if NIGHTLY
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.Updater.Releases.Models;
using SoundSwitch.Util;

namespace SoundSwitch.Tests;

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
}
#endif
