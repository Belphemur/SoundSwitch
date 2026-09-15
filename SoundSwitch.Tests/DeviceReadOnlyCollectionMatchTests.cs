#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Collection;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Tests;

/// <summary>
/// Tests the 3-tier matching exposed by <see cref="DeviceReadOnlyCollection{T}.Match"/>:
/// exact Id, stable device-instance path (survives endpoint-GUID churn from driver updates),
/// then unique <see cref="DeviceInfo.NameClean"/>. Ambiguous names are reported, never guessed.
/// </summary>
[TestFixture]
public class DeviceReadOnlyCollectionMatchTests
{
    private const string RenderFlow = "{0.0.0.00000000}.";

    private static DeviceFullInfo Device(string name, string id, EDataFlow type) =>
        new(name, id, type, "", EDeviceState.Active, false);

    private const string SpeakersName = "Speakers (Realtek(R) Audio)";
    private const string HeadphonesName = "Headphones (Realtek(R) Audio)";

    // 3-segment endpoint ids: {flow}.{device-instance-path}.{endpoint-guid}
    private const string SpeakersPath = @"USB\VID_1234&PID_5678&MI_00\6&2a3f5d7&0&0000";
    private const string SpeakersEndpointGuid = "{aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa}";
    private const string SpeakersNewEndpointGuid = "{cccccccc-cccc-cccc-cccc-cccccccccccc}";

    private static string ThreeSegmentId(string path, string endpointGuid) => $"{RenderFlow}{{{path}}}.{endpointGuid}";

    [Test]
    public void GetInstancePath_ThreeSegmentId_ExtractsMiddleSegment()
    {
        // The device-instance path itself contains dots and must survive extraction.
        var id = ThreeSegmentId(@"HDAUDIO\FUNC_01&DEV_0002&SUBSYS_10EC&REV_1001.4&2a3f5d7&0&0101", SpeakersEndpointGuid);

        DeviceReadOnlyCollection<DeviceFullInfo>.GetInstancePath(id)
            .Should().Be(@"HDAUDIO\FUNC_01&DEV_0002&SUBSYS_10EC&REV_1001.4&2a3f5d7&0&0101");
    }

    [Test]
    public void GetInstancePath_TwoSegmentId_ReturnsNull()
    {
        DeviceReadOnlyCollection<DeviceFullInfo>.GetInstancePath($"{RenderFlow}{{11111111-1111-1111-1111-111111111111}}")
            .Should().BeNull();
    }

    [Test]
    public void GetInstancePath_NullOrMalformed_ReturnsNull()
    {
        DeviceReadOnlyCollection<DeviceFullInfo>.GetInstancePath(null).Should().BeNull();
        DeviceReadOnlyCollection<DeviceFullInfo>.GetInstancePath("").Should().BeNull();
        DeviceReadOnlyCollection<DeviceFullInfo>.GetInstancePath("not-an-id").Should().BeNull();
    }

    [Test]
    public void Match_ExactId_ReturnsExactId()
    {
        var id = ThreeSegmentId(SpeakersPath, SpeakersEndpointGuid);
        var collection = new DeviceReadOnlyCollection<DeviceFullInfo>(
            new[] { Device(SpeakersName, id, EDataFlow.eRender) }, EDataFlow.eRender);

        var result = collection.Match(new DeviceInfo(SpeakersName, id, EDataFlow.eRender, false, DateTime.UtcNow));

        result.Kind.Should().Be(DeviceMatchKind.ExactId);
        result.Device!.Id.Should().Be(id);
    }

    [Test]
    public void Match_EndpointGuidChanged_StablePathStillMatches()
    {
        // After a driver update the endpoint GUID changed but the instance path stayed.
        var oldId = ThreeSegmentId(SpeakersPath, SpeakersEndpointGuid);
        var newId = ThreeSegmentId(SpeakersPath, SpeakersNewEndpointGuid);
        var collection = new DeviceReadOnlyCollection<DeviceFullInfo>(
            new[] { Device(SpeakersName, newId, EDataFlow.eRender) }, EDataFlow.eRender);

        var result = collection.Match(new DeviceInfo(SpeakersName, oldId, EDataFlow.eRender, false, DateTime.UtcNow));

        result.Kind.Should().Be(DeviceMatchKind.StablePath);
        result.Device!.Id.Should().Be(newId);
    }

    [Test]
    public void Match_StablePath_RequiresSameType()
    {
        // Same instance path captured by a capture endpoint: a render device must not match it.
        var oldId = ThreeSegmentId(SpeakersPath, SpeakersEndpointGuid);
        var collection = new DeviceReadOnlyCollection<DeviceFullInfo>(
            new[] { Device(SpeakersName, ThreeSegmentId(SpeakersPath, SpeakersNewEndpointGuid), EDataFlow.eCapture) },
            EDataFlow.eRender);

        collection.Match(new DeviceInfo(SpeakersName, oldId, EDataFlow.eRender, false, DateTime.UtcNow))
            .Kind.Should().Be(DeviceMatchKind.None);
    }

    [Test]
    public void Match_TwoSegmentIds_StablePathIsNoOp_FallsBackToName()
    {
        // Virtual devices have no instance path: nothing to match on, falls through to the name.
        var oldId = $"{RenderFlow}{{11111111-1111-1111-1111-111111111111}}";
        var newId = $"{RenderFlow}{{22222222-2222-2222-2222-222222222222}}";
        var collection = new DeviceReadOnlyCollection<DeviceFullInfo>(
            new[] { Device(SpeakersName, newId, EDataFlow.eRender) }, EDataFlow.eRender);

        var result = collection.Match(new DeviceInfo(SpeakersName, oldId, EDataFlow.eRender, false, DateTime.UtcNow));

        result.Kind.Should().Be(DeviceMatchKind.UniqueName);
        result.Device!.Id.Should().Be(newId);
    }

    [Test]
    public void Match_UniqueName_ReturnsUniqueName()
    {
        var collection = new DeviceReadOnlyCollection<DeviceFullInfo>(
            new[] { Device(HeadphonesName, $"{RenderFlow}{{22222222-2222-2222-2222-222222222222}}", EDataFlow.eRender) },
            EDataFlow.eRender);

        var stored = new DeviceInfo(HeadphonesName, $"{RenderFlow}{{11111111-1111-1111-1111-111111111111}}", EDataFlow.eRender, false, DateTime.UtcNow);

        collection.Match(stored).Kind.Should().Be(DeviceMatchKind.UniqueName);
    }

    [Test]
    public void Match_DuplicateNameClean_ReturnsAmbiguousWithAllCandidates()
    {
        // Two identical USB units: NameCleanerRegex strips the "(2)" suffix, so both share a NameClean.
        // The grouping must keep both candidates (not last-wins) and report the ambiguity.
        var first = Device(SpeakersName, ThreeSegmentId(SpeakersPath, SpeakersEndpointGuid), EDataFlow.eRender);
        var second = Device(SpeakersName, ThreeSegmentId(@"USB\VID_1234&PID_5678&MI_00\6&2a3f5d7&0&0001", SpeakersEndpointGuid), EDataFlow.eRender);
        var collection = new DeviceReadOnlyCollection<DeviceFullInfo>(new[] { first, second }, EDataFlow.eRender);

        var stored = new DeviceInfo(SpeakersName, $"{RenderFlow}{{99999999-9999-9999-9999-999999999999}}", EDataFlow.eRender, false, DateTime.UtcNow);

        var result = collection.Match(stored);
        result.Kind.Should().Be(DeviceMatchKind.Ambiguous);
        result.Device.Should().BeNull();
        result.Candidates.Should().BeEquivalentTo(new[] { first, second });
    }

    [Test]
    public void Match_TypeMismatch_ReturnsNone()
    {
        var id = ThreeSegmentId(SpeakersPath, SpeakersEndpointGuid);
        var collection = new DeviceReadOnlyCollection<DeviceFullInfo>(
            new[] { Device(SpeakersName, id, EDataFlow.eCapture) }, EDataFlow.eCapture);

        collection.Match(new DeviceInfo(SpeakersName, id, EDataFlow.eRender, false, DateTime.UtcNow))
            .Kind.Should().Be(DeviceMatchKind.None);
        collection.Match(new DeviceInfo(HeadphonesName, $"{RenderFlow}{{11111111-1111-1111-1111-111111111111}}", EDataFlow.eCapture, false, DateTime.UtcNow))
            .Kind.Should().Be(DeviceMatchKind.None);
    }

    [Test]
    public void IntersectWith_AmbiguousName_SkipsAllCandidates()
    {
        // Bug fix vs the previous last-wins behaviour: with two identical units the
        // ambiguous pair is skipped instead of silently keeping an arbitrary one.
        var selected = new[]
        {
            Device(SpeakersName, ThreeSegmentId(SpeakersPath, SpeakersEndpointGuid), EDataFlow.eRender),
            Device(SpeakersName, ThreeSegmentId(@"USB\VID_1234&PID_5678&MI_00\6&2a3f5d7&0&0001", SpeakersEndpointGuid), EDataFlow.eRender)
        };
        var collection = new DeviceReadOnlyCollection<DeviceFullInfo>(selected, EDataFlow.eRender);

        var stored = new DeviceInfo(SpeakersName, $"{RenderFlow}{{99999999-9999-9999-9999-999999999999}}", EDataFlow.eRender, false, DateTime.UtcNow);

        collection.IntersectWith(new[] { stored }).Should().BeEmpty();
    }

    [Test]
    public void IntersectWith_ResolvedTiers_ReturnsLiveDevices()
    {
        var newId = ThreeSegmentId(SpeakersPath, SpeakersNewEndpointGuid);
        var collection = new DeviceReadOnlyCollection<DeviceFullInfo>(
            new[]
            {
                Device(SpeakersName, newId, EDataFlow.eRender),
                Device(HeadphonesName, $"{RenderFlow}{{22222222-2222-2222-2222-222222222222}}", EDataFlow.eRender)
            }, EDataFlow.eRender);

        var selection = new DeviceInfo[]
        {
            new(SpeakersName, ThreeSegmentId(SpeakersPath, SpeakersEndpointGuid), EDataFlow.eRender, false, DateTime.UtcNow),
            new(HeadphonesName, $"{RenderFlow}{{11111111-1111-1111-1111-111111111111}}", EDataFlow.eRender, false, DateTime.UtcNow),
            new("Gone (Other)", $"{RenderFlow}{{33333333-3333-3333-3333-333333333333}}", EDataFlow.eRender, false, DateTime.UtcNow)
        };

        var matched = collection.IntersectWith(selection).ToList();

        matched.Should().HaveCount(2);
        matched.Should().Contain(d => d.Id == newId);
        matched.Should().Contain(d => d.NameClean == HeadphonesName);
    }

    [Test]
    public void IntersectWith_WrongFlow_CandidatesFiltered()
    {
        var id = ThreeSegmentId(SpeakersPath, SpeakersEndpointGuid);
        var collection = new DeviceReadOnlyCollection<DeviceFullInfo>(
            new[] { Device(SpeakersName, id, EDataFlow.eRender) }, EDataFlow.eRender);

        var captureDevice = new DeviceInfo(SpeakersName, id, EDataFlow.eCapture, false, DateTime.UtcNow);

        collection.IntersectWith(new[] { captureDevice }).Should().BeEmpty();
    }
}
