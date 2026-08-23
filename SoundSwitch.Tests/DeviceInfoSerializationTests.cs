using System;

using FluentAssertions;

using NAudio.CoreAudioApi;

using Newtonsoft.Json;

using NUnit.Framework;

using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Tests;

/// <summary>
/// Characterization tests (Phase 0 of the NAudio removal plan): pin the numeric JSON wire form of
/// the NAudio enums carried by the device DTOs (<see cref="DeviceInfo.Type"/> and
/// <see cref="DeviceFullInfo.State"/>). The configuration path serializes with Newtonsoft.Json and
/// registers no StringEnumConverter (ConfigurationManager only sets NullValueHandling.Ignore), so
/// enums are written as integers. The later swap of NAudio's DataFlow/DeviceState for in-house
/// EDataFlow/EDeviceState must round-trip to the same numeric values.
/// </summary>
[TestFixture]
public class DeviceInfoSerializationTests
{
    private static readonly DateTime DiscoveredAt = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public void NAudioEnums_HaveExpectedNumericValues()
    {
        // These are the values the in-house EDataFlow/EDeviceState enums must keep.
        ((int)DataFlow.Render).Should().Be(0);
        ((int)DataFlow.Capture).Should().Be(1);
        ((int)DeviceState.Active).Should().Be(1);
        ((int)DeviceState.All).Should().Be(0xF);
    }

    [Test]
    public void DeviceInfo_Type_SerializesAsInteger()
    {
        var device = new DeviceInfo("Speakers (Realtek(R) Audio)", "{0.0.0.00000000}.{11111111-1111-1111-1111-111111111111}", DataFlow.Render, true, DiscoveredAt);

        var json = JsonConvert.SerializeObject(device);

        json.Should().Contain("\"Type\":0");
        json.Should().NotContain("\"Type\":\"Render\"");

        var deserialized = JsonConvert.DeserializeObject<DeviceInfo>(json);

        deserialized.Should().NotBeNull();
        deserialized!.Type.Should().Be(DataFlow.Render);
        deserialized.Should().Be(device);
    }

    [Test]
    public void DeviceInfo_Type_Capture_SerializesAsIntegerOne()
    {
        var device = new DeviceInfo("Microphone (Realtek(R) Audio)", "{0.0.1.00000000}.{22222222-2222-2222-2222-222222222222}", DataFlow.Capture, false, DiscoveredAt);

        var json = JsonConvert.SerializeObject(device);

        json.Should().Contain("\"Type\":1");
        json.Should().NotContain("\"Type\":\"Capture\"");

        var deserialized = JsonConvert.DeserializeObject<DeviceInfo>(json);

        deserialized.Should().NotBeNull();
        deserialized!.Type.Should().Be(DataFlow.Capture);
    }

    [Test]
    public void DeviceFullInfo_State_And_Type_SerializeAsIntegers()
    {
        var device = new DeviceFullInfo("Speakers (Realtek(R) Audio)", "{0.0.0.00000000}.{11111111-1111-1111-1111-111111111111}", DataFlow.Render, @"C:\Windows\speaker.ico", DeviceState.Active, true);

        var json = JsonConvert.SerializeObject(device);

        json.Should().Contain("\"Type\":0");
        json.Should().Contain("\"State\":1");
        json.Should().NotContain("\"Type\":\"Render\"");
        json.Should().NotContain("\"State\":\"Active\"");

        var deserialized = JsonConvert.DeserializeObject<DeviceFullInfo>(json);

        deserialized.Should().NotBeNull();
        deserialized!.Type.Should().Be(DataFlow.Render);
        deserialized.State.Should().Be(DeviceState.Active);
        deserialized.IconPath.Should().Be(device.IconPath);
    }
}
