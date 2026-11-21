using System;
using FluentAssertions;
using NUnit.Framework;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Framework.TrayIcon;
using SoundSwitch.Framework.TrayIcon.IconChanger;
using SoundSwitch.Framework.TrayIcon.IconChanger.Changer;
using SoundSwitch.Model;

namespace SoundSwitch.Tests;

/// <summary>
/// Regression tests for issue #2620: the Microphone Mute State tray icon type keys off
/// the tracked microphone's mute state, not the default device.
/// </summary>
[TestFixture]
public class IconChangerMuteStateTests
{
    [Test]
    public void SelectIconKind_Muted_SelectsMutedGlyph()
    {
        IconChangerMuteState.SelectIconKind(true).Should().Be(IconKind.MicrophoneMuted);
    }

    [Test]
    public void SelectIconKind_Unmuted_SelectsMicrophoneGlyph()
    {
        IconChangerMuteState.SelectIconKind(false).Should().Be(IconKind.Microphone);
    }

    [Test]
    public void Muted_And_Unmuted_Glyphs_Differ()
    {
        using var muted = ThemeIcons.GetIcon(IconChangerMuteState.SelectIconKind(true), false).Acquire();
        using var unmuted = ThemeIcons.GetIcon(IconChangerMuteState.SelectIconKind(false), false).Acquire();

        muted.Icon.Should().NotBeNull();
        unmuted.Icon.Should().NotBeNull();
        muted.Icon.Should().NotBeSameAs(unmuted.Icon);
    }

    [Test]
    public void DeviceDrivenOverload_IsNoOp()
    {
        var changer = new IconChangerMuteState();

        // The device-driven overload must never touch the tray icon: passing one
        // through unchanged (here: null) is the observable no-op contract.
        Action act = () => changer.ChangeIcon(null, null, ERole.eCommunications);
        act.Should().NotThrow();
    }

    [Test]
    public void Factory_ResolvesMuteStateChanger()
    {
        var changer = new IconChangerFactory().Get(IconChanger.MuteState);
        changer.Should().BeOfType<IconChangerMuteState>();
        changer.TypeEnum.Should().Be(IconChanger.MuteState);
        changer.Label.Should().NotBeNullOrEmpty();
    }

    [TestCase("id.1", "id.1", true)]
    [TestCase("id.1", "ID.1", true)]
    [TestCase("id.1", "id.2", false)]
    [TestCase("id.1", null, false)]
    [TestCase("id.1", "", false)]
    public void IsTrackedMuteDevice_FiltersToResolvedTarget(string deviceId, string targetId, bool expected)
    {
        DeviceService.IsTrackedMuteDevice(deviceId, targetId).Should().Be(expected);
    }
}
