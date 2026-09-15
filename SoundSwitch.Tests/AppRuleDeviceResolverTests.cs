using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Services;

namespace SoundSwitch.Tests;

/// <summary>
/// Tests the device resolution used by App Rules: exact id match first,
/// then the <see cref="DeviceInfo.Equals"/> (NameClean) fallback — the same
/// matching machinery Profiles use, which survives device id changes.
/// </summary>
[TestFixture]
public class AppRuleDeviceResolverTests
{
    private const string PlaybackId = "{0.0.0.00000000}.{11111111-1111-1111-1111-111111111111}";
    private const string PlaybackNewId = "{0.0.0.00000000}.{33333333-3333-3333-3333-333333333333}";
    private const string OtherPlaybackId = "{0.0.0.00000000}.{22222222-2222-2222-2222-222222222222}";
    private const string SpeakersName = "Speakers (Realtek(R) Audio)";

    private static DeviceFullInfo Device(string name, string id, EDataFlow type) =>
        new(name, id, type, "", EDeviceState.Active, false);

    private static readonly DeviceInfo StoredSpeakers = new(SpeakersName, PlaybackId, EDataFlow.eRender, false, DateTime.UtcNow);

    [Test]
    public void Resolve_ExactIdMatch_WinsOverEarlierSameNameCleanCandidate()
    {
        // Same NameClean but a different id comes FIRST in the list: it must not
        // shadow the candidate actually carrying the stored id.
        var candidates = new[]
        {
            Device(SpeakersName, OtherPlaybackId, EDataFlow.eRender),
            Device(SpeakersName, PlaybackId, EDataFlow.eRender)
        };

        var resolved = AppRuleDeviceResolver.Resolve(StoredSpeakers, candidates);

        resolved.Should().NotBeNull();
        resolved!.Id.Should().Be(PlaybackId);
    }

    [Test]
    public void Resolve_IdChanged_FallsBackToNameClean()
    {
        // After a driver update the device id changed but the device kept its name.
        var candidates = new[]
        {
            Device("Headphones", OtherPlaybackId, EDataFlow.eRender),
            Device(SpeakersName, PlaybackNewId, EDataFlow.eRender)
        };

        var resolved = AppRuleDeviceResolver.Resolve(StoredSpeakers, candidates);

        resolved.Should().NotBeNull();
        resolved!.Id.Should().Be(PlaybackNewId);
    }

    [Test]
    public void Resolve_TypeMismatch_DoesNotMatch()
    {
        var candidates = new[] { Device(SpeakersName, PlaybackId, EDataFlow.eCapture) };

        AppRuleDeviceResolver.Resolve(StoredSpeakers, candidates).Should().BeNull();
    }

    [Test]
    public void Resolve_NoCandidateMatches_ReturnsNull()
    {
        var candidates = new[] { Device("Headphones", OtherPlaybackId, EDataFlow.eRender) };

        AppRuleDeviceResolver.Resolve(StoredSpeakers, candidates).Should().BeNull();
    }

    [Test]
    public void Resolve_NullStored_ReturnsNull()
    {
        var candidates = new[] { Device(SpeakersName, PlaybackId, EDataFlow.eRender) };

        AppRuleDeviceResolver.Resolve(null, candidates).Should().BeNull();
    }

    [Test]
    public void Resolve_EmptyCandidates_ReturnsNull()
    {
        AppRuleDeviceResolver.Resolve(StoredSpeakers, Enumerable.Empty<DeviceFullInfo>()).Should().BeNull();
    }

    [Test]
    public void Resolve_IdMatch_RequiresSameTypeBeforeNameCleanFallback()
    {
        // A candidate with the same id but a different type must not match on id;
        // the NameClean fallback also requires the same type, so nothing matches.
        var candidates = new[] { Device(SpeakersName, PlaybackId, EDataFlow.eCapture) };

        AppRuleDeviceResolver.Resolve(StoredSpeakers, candidates).Should().BeNull();
    }
}
