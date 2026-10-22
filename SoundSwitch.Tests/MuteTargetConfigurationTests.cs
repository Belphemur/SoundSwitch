using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

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
        config.MuteRecordingDeviceId.Should().BeNull();
        config.LimitMuteBannersToTarget.Should().BeTrue();
        config.MigratedFields.Should().Contain(nameof(SoundSwitchConfiguration.MuteRecordingDeviceId));
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
        var config = new SoundSwitchConfiguration { MuteRecordingDeviceId = deviceId };

        var restored = JsonConvert.DeserializeObject<SoundSwitchConfiguration>(JsonConvert.SerializeObject(config));

        restored.Should().NotBeNull();
        restored.Migrate().Should().BeTrue();
        restored.MuteRecordingDeviceId.Should().Be(deviceId);
    }
}
