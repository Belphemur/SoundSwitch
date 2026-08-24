using System;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Profile;

namespace SoundSwitch.Tests;

/// <summary>
/// Pins the profile → ERole matching contract fixed after review: <see cref="Profile.Devices"/>
/// exposes each device entry with the exact set of native roles it must react to, and consumers
/// match with <c>Roles.Contains</c>. The previous implementation OR-ed roles and used HasFlag,
/// which — because eConsole = 0 and eMultimedia = 1 — made a playback-only profile match a
/// communications-role switch. These tests document why flags arithmetic can never come back.
/// </summary>
[TestFixture]
public class ProfileRoleMatchingTests
{
    private static DeviceInfo Device(string name) =>
        new(name, $"{name}-id", EDataFlow.eRender, false, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Test]
    public void NativeEroleValues_CannotBeCombinedWithBitwiseOperators()
    {
        // OR-ing eConsole (= 0) into eMultimedia is a no-op …
        (ERole.eConsole | ERole.eMultimedia).Should().Be(ERole.eMultimedia);
        // … and because eConsole == 0, HasFlag(eConsole) is true for ANY value.
        ERole.eCommunications.HasFlag(ERole.eConsole).Should().BeTrue();
        ((ERole)99u).HasFlag(ERole.eConsole).Should().BeTrue();
    }

    [Test]
    public void PlaybackDevice_CoversConsoleAndMultimedia_ButNotCommunications()
    {
        var profile = new Profile { Playback = Device("speakers") };

        var wrapper = profile.Devices.Single();

        wrapper.DeviceInfo.Should().Be(profile.Playback);
        wrapper.Roles.Should().Contain(ERole.eConsole);
        wrapper.Roles.Should().Contain(ERole.eMultimedia);
        wrapper.Roles.Should().NotContain(ERole.eCommunications);
    }

    [Test]
    public void CommunicationDevice_CoversOnlyCommunicationsRole()
    {
        var profile = new Profile { Communication = Device("headset") };

        var wrapper = profile.Devices.Single();

        wrapper.DeviceInfo.Should().Be(profile.Communication);
        wrapper.Roles.Should().Equal(ERole.eCommunications);
    }

    [Test]
    public void AllFourDevicesSet_YieldsOneWrapperPerDeviceInDeclarationOrder()
    {
        var profile = new Profile
        {
            Playback = Device("speakers"),
            Communication = Device("headset"),
            Recording = Device("microphone"),
            RecordingCommunication = Device("chat-microphone"),
        };

        var wrappers = profile.Devices.ToList();

        wrappers.Should().HaveCount(4);
        wrappers[0].DeviceInfo.Should().Be(profile.Playback);
        wrappers[0].Roles.Should().Equal(ERole.eConsole, ERole.eMultimedia);
        wrappers[1].DeviceInfo.Should().Be(profile.Communication);
        wrappers[1].Roles.Should().Equal(ERole.eCommunications);
        wrappers[2].DeviceInfo.Should().Be(profile.Recording);
        wrappers[2].Roles.Should().Equal(ERole.eConsole, ERole.eMultimedia);
        wrappers[3].DeviceInfo.Should().Be(profile.RecordingCommunication);
        wrappers[3].Roles.Should().Equal(ERole.eCommunications);
    }

    [Test]
    public void ProfileWithoutDevices_YieldsNoDevices()
    {
        new Profile().Devices.Should().BeEmpty();
    }
}
