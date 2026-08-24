using System;

using FluentAssertions;

using Newtonsoft.Json;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Tests;

/// <summary>
/// Characterization tests (Phases 0–1 of the removal plan): pin the numeric JSON wire form
/// of the in-house enums carried by the device DTOs (<see cref="DeviceInfo.Type"/> and
/// <see cref="DeviceFullInfo.State"/>). The configuration path serializes with Newtonsoft.Json and
/// registers no StringEnumConverter (ConfigurationManager only sets NullValueHandling.Ignore), so
/// enums are written as integers. The swap from the legacy DataFlow/DeviceState to the in-house
/// EDataFlow/EDeviceState keeps the same numeric values (eRender=0, eCapture=1; Active=1,
/// All=0xF) and therefore round-trips identically.
/// </summary>
[TestFixture]
public class DeviceInfoSerializationTests
{
    private static readonly DateTime DiscoveredAt = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public void InHouseEnums_HaveExpectedNumericValues()
    {
        // These are the values the in-house EDataFlow/EDeviceState enums must keep (identical to
        // the legacy enums they replace, and to the native EDataFlow/DEVICE_STATE constants).
        ((int)EDataFlow.eRender).Should().Be(0);
        ((int)EDataFlow.eCapture).Should().Be(1);
        ((int)EDeviceState.Active).Should().Be(1);
        ((int)EDeviceState.All).Should().Be(0xF);
    }

    [Test]
    public void DeviceInfo_Type_SerializesAsInteger()
    {
        var device = new DeviceInfo("Speakers (Realtek(R) Audio)", "{0.0.0.00000000}.{11111111-1111-1111-1111-111111111111}", EDataFlow.eRender, true, DiscoveredAt);

        var json = JsonConvert.SerializeObject(device);

        json.Should().Contain("\"Type\":0");
        json.Should().NotContain("\"Type\":\"Render\"");

        var deserialized = JsonConvert.DeserializeObject<DeviceInfo>(json);

        deserialized.Should().NotBeNull();
        deserialized!.Type.Should().Be(EDataFlow.eRender);
        deserialized.Should().Be(device);
    }

    [Test]
    public void DeviceInfo_Type_Capture_SerializesAsIntegerOne()
    {
        var device = new DeviceInfo("Microphone (Realtek(R) Audio)", "{0.0.1.00000000}.{22222222-2222-2222-2222-222222222222}", EDataFlow.eCapture, false, DiscoveredAt);

        var json = JsonConvert.SerializeObject(device);

        json.Should().Contain("\"Type\":1");
        json.Should().NotContain("\"Type\":\"Capture\"");

        var deserialized = JsonConvert.DeserializeObject<DeviceInfo>(json);

        deserialized.Should().NotBeNull();
        deserialized!.Type.Should().Be(EDataFlow.eCapture);
    }

    [Test]
    public void DeviceFullInfo_State_And_Type_SerializeAsIntegers()
    {
        var device = new DeviceFullInfo("Speakers (Realtek(R) Audio)", "{0.0.0.00000000}.{11111111-1111-1111-1111-111111111111}", EDataFlow.eRender, @"C:\Windows\speaker.ico", EDeviceState.Active, true);

        var json = JsonConvert.SerializeObject(device);

        json.Should().Contain("\"Type\":0");
        json.Should().Contain("\"State\":1");
        json.Should().NotContain("\"Type\":\"Render\"");
        json.Should().NotContain("\"State\":\"Active\"");

        var deserialized = JsonConvert.DeserializeObject<DeviceFullInfo>(json);

        deserialized.Should().NotBeNull();
        deserialized!.Type.Should().Be(EDataFlow.eRender);
        deserialized.State.Should().Be(EDeviceState.Active);
        deserialized.IconPath.Should().Be(device.IconPath);
    }
}
