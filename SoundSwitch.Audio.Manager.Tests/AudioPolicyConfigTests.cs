using System;
using FluentAssertions;
using NUnit.Framework;
using SoundSwitch.Audio.Manager.Interop.Client.Extended;
using SoundSwitch.Audio.Manager.Interop.Client.Extended.Factory;
using SoundSwitch.Audio.Manager;

namespace SoundSwitch.Audio.Manager.Tests;

[TestFixture]
public sealed class AudioPolicyConfigTests
{
    [Test]
    public void CreateAudioPolicyConfigTest()
    {
        Assert.DoesNotThrow(() => { AudioPolicyConfigFactory.Create(); });
    }

    [Test]
    public void UnsupportedWindowsVersionTest()
    {
        var audioPolicyConfig = AudioPolicyConfigFactory.Create();
        if (Environment.OSVersion.Version.Major < 10 || Environment.OSVersion.Version.Build <= AudioPolicyConfigFactory.OS_1709_VERSION)

        {
            var noConfig = audioPolicyConfig is UnsupportedAudioPolicyConfig;
            noConfig.Should().BeTrue($"audioPolicyConfig should be {nameof(UnsupportedAudioPolicyConfig)} if not on Windows 10 versions above 1709");
        }
        else
        {
            var foundConfig = audioPolicyConfig is AudioPolicyConfig;
            foundConfig.Should().BeTrue($"audioPolicyConfig should be a valid {nameof(AudioPolicyConfig)}");
        }
    }

    [Test]
    public void SwitchDefaultAudioDeviceTest()
    {
        var audioSwitcher = AudioSwitcher.Instance;
        var defaultDevice = audioSwitcher.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole);
        var devices = audioSwitcher.GetAudioEndpoints(EDataFlow.eRender, EDeviceState.Active);

        if (devices.Count < 2)
        {
            Assert.Inconclusive("Not enough audio devices to perform the switch test.");
        }

        var newDefaultDevice = devices.FirstOrDefault(d => d.Id != defaultDevice?.Id);
        if (newDefaultDevice == null)
        {
            Assert.Inconclusive("No alternative audio device found to switch to.");
        }

        audioSwitcher.SwitchTo(newDefaultDevice.Id, ERole.eConsole);
        var switchedDevice = audioSwitcher.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole);

        switchedDevice.Should().NotBeNull();
        switchedDevice?.Id.Should().Be(newDefaultDevice.Id);

        // Revert to the original default device
        if (defaultDevice != null)
        {
            audioSwitcher.SwitchTo(defaultDevice.Id, ERole.eConsole);
        }
    }
}
