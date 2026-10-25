using System;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Configuration;

namespace SoundSwitch.Tests;

[TestFixture]
public class MuteTargetConfigurationTests
{
    [Test]
    public void LegacyConfiguration_UsesDefaultInputAndTargetOnlyBanners()
    {
        var config = JsonConvert.DeserializeObject<SoundSwitchConfiguration>("{}");

        config.Should().NotBeNull();
        config.Migrate().Should().BeTrue();
        config.MuteRecordingDevice.Should().BeNull();
        config.LimitMuteBannersToTarget.Should().BeTrue();
    }

    [Test]
    public void AnyMicrophoneBanners_SurviveConfigurationRoundTrip()
    {
        var config = new SoundSwitchConfiguration { LimitMuteBannersToTarget = false };

        var restored = JsonConvert.DeserializeObject<SoundSwitchConfiguration>(JsonConvert.SerializeObject(config));

        restored.Should().NotBeNull();
        restored.LimitMuteBannersToTarget.Should().BeFalse();
    }

    [Test]
    public void SelectedMicrophone_SurvivesConfigurationRoundTrip()
    {
        const string deviceId = "{0.0.1.00000000}.{59e4df6f-d9a6-4ce0-9899-49dede000000}";
        var device = new DeviceInfo("Microphone (USB Audio)", deviceId, EDataFlow.eCapture, true, DateTime.UtcNow);
        var config = new SoundSwitchConfiguration { MuteRecordingDevice = device };

        var restored = JsonConvert.DeserializeObject<SoundSwitchConfiguration>(JsonConvert.SerializeObject(config));

        restored.Should().NotBeNull();
        restored.MuteRecordingDevice.Should().NotBeNull();
        restored.MuteRecordingDevice!.Id.Should().Be(deviceId);
        restored.MuteRecordingDevice.Type.Should().Be(EDataFlow.eCapture);
        restored.MuteRecordingDevice.IsUsb.Should().BeTrue();
        restored.MuteRecordingDevice.NameClean.Should().Be(device.NameClean);
    }
}
