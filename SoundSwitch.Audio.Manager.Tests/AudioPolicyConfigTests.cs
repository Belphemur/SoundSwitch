using System;
using FluentAssertions;
using NUnit.Framework;
using SoundSwitch.Audio.Manager.Interop.Client.Extended;
using SoundSwitch.Audio.Manager.Interop.Client.Extended.Factory;

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
}