using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Collection;
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

    private const string SpeakersPath = @"USB\VID_1234&PID_5678&MI_00\6&2a3f5d7&0&0000";
    private const string SpeakersPathB = @"USB\VID_1234&PID_5678&MI_00\6&2a3f5d7&0&0001";
    private const string SpeakersEndpointGuid = "{aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa}";
    private const string SpeakersNewEndpointGuid = "{cccccccc-cccc-cccc-cccc-cccccccccccc}";

    private static string ThreeSegmentId(string path, string endpointGuid) =>
        $"{{0.0.0.00000000}}.{{{path}}}.{endpointGuid}";

    [Test]
    public void Resolve_EndpointGuidChanged_ResolvedViaStablePath()
    {
        // The endpoint GUID changed (driver update) but the device-instance path is stable:
        // resolution must still succeed even though the NameClean differs too.
        var stored = new DeviceInfo("Old name (Realtek(R) Audio)", ThreeSegmentId(SpeakersPath, SpeakersEndpointGuid), EDataFlow.eRender, false, DateTime.UtcNow);
        var candidates = new[] { Device("New name (Realtek(R) Audio)", ThreeSegmentId(SpeakersPath, SpeakersNewEndpointGuid), EDataFlow.eRender) };

        var resolved = AppRuleDeviceResolver.Resolve(stored, candidates);

        resolved.Should().NotBeNull();
        resolved!.Id.Should().Be(ThreeSegmentId(SpeakersPath, SpeakersNewEndpointGuid));
    }

    [Test]
    public void Resolve_DuplicateNameClean_ReturnsNullAmbiguous()
    {
        // Two identical units share the stored NameClean and none carries the stored id:
        // the resolver must warn-and-skip, never guess a unit.
        var stored = new DeviceInfo(SpeakersName, $"{{0.0.0.00000000}}.{{99999999-9999-9999-9999-999999999999}}", EDataFlow.eRender, false, DateTime.UtcNow);
        var candidates = new[]
        {
            Device(SpeakersName, ThreeSegmentId(SpeakersPath, SpeakersEndpointGuid), EDataFlow.eRender),
            Device(SpeakersName, ThreeSegmentId(SpeakersPathB, SpeakersEndpointGuid), EDataFlow.eRender)
        };

        AppRuleDeviceResolver.Resolve(stored, candidates).Should().BeNull();
    }

    [Test]
    public void ResolveResult_DuplicateNameClean_ReportsAmbiguousWithCandidates()
    {
        var stored = new DeviceInfo(SpeakersName, $"{{0.0.0.00000000}}.{{99999999-9999-9999-9999-999999999999}}", EDataFlow.eRender, false, DateTime.UtcNow);
        var candidates = new[]
        {
            Device(SpeakersName, $"{{0.0.0.00000000}}.{{11111111-1111-1111-1111-111111111111}}", EDataFlow.eRender),
            Device(SpeakersName, $"{{0.0.0.00000000}}.{{22222222-2222-2222-2222-222222222222}}", EDataFlow.eRender)
        };

        var result = AppRuleDeviceResolver.ResolveResult(stored, candidates);

        result.Kind.Should().Be(DeviceMatchKind.Ambiguous);
        result.Candidates.Should().HaveCount(2);
    }
}
