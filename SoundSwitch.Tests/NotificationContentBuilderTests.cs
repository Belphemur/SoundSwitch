/********************************************************************
 * Copyright (C) 2015-2024 Antoine Aflalo
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 ********************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;

using SoundSwitch.Audio.Manager.Interop.Enum;

using NUnit.Framework;
using FluentAssertions;

using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.NotificationManager.Notification;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Localization;
using SoundSwitch.Model;

namespace SoundSwitch.Tests;

[TestFixture]
public class NotificationContentBuilderTests
{
    private static DeviceFullInfo CreateDevice(string name, EDataFlow type) =>
        new(name, $"id-{name}", type, @"C:\nonexistent\device.ico", EDeviceState.Active, false);

    private static DeviceInfo CreateStoredDevice(string name, EDataFlow type) =>
        new(name, $"stored-{name}", type, false, DateTime.UtcNow);

    [Test]
    public void BuildDefaultChanged_RenderWindowsNotification_SetsTitleTextAndImage()
    {
        var device = CreateDevice("Test Speaker", EDataFlow.eRender);

        var data = NotificationContentBuilder.BuildDefaultChanged(device, NotificationContentBuilder.DeviceChangeWording.WindowsNotification);

        data.Title.Should().Be(TrayIconStrings.playbackChanged);
        data.Text.Should().Be(device.NameClean);
        data.Image.Should().NotBeNull();
    }

    [Test]
    public void BuildDefaultChanged_RenderBanner_SetsPlaybackDeviceTitle()
    {
        var device = CreateDevice("Test Speaker", EDataFlow.eRender);

        var data = NotificationContentBuilder.BuildDefaultChanged(device, NotificationContentBuilder.DeviceChangeWording.Banner);

        data.Title.Should().Be(SettingsStrings.tooltipOnHover_option_playbackDevice);
    }

    [Test]
    public void BuildDefaultChanged_CaptureWindowsNotification_SetsRecordingTitle()
    {
        var device = CreateDevice("Test Microphone", EDataFlow.eCapture);

        var data = NotificationContentBuilder.BuildDefaultChanged(device, NotificationContentBuilder.DeviceChangeWording.WindowsNotification);

        data.Title.Should().Be(TrayIconStrings.recordingChanged);
        data.Text.Should().Be(device.NameClean);
    }

    [Test]
    public void BuildDefaultChanged_CaptureBanner_SetsRecordingDeviceTitle()
    {
        var device = CreateDevice("Test Microphone", EDataFlow.eCapture);

        var data = NotificationContentBuilder.BuildDefaultChanged(device, NotificationContentBuilder.DeviceChangeWording.Banner);

        data.Title.Should().Be(SettingsStrings.tooltipOnHover_option_recordingDevice);
    }

    [Test]
    public void BuildProfileChanged_SetsPriorityTitleAndText()
    {
        var profile = new Profile
        {
            Name = "Gaming",
            Playback = new DeviceInfo("Speakers", "id-speakers", EDataFlow.eRender, false, DateTime.UtcNow),
            Recording = new DeviceInfo("Mic", "id-mic", EDataFlow.eCapture, false, DateTime.UtcNow)
        };

        var data = NotificationContentBuilder.BuildProfileChanged(profile, new Bitmap(1, 1));

        data.Priority.Should().Be(1);
        data.Title.Should().Contain(profile.Name);
        data.Text.Should().Contain("Speakers");
        data.Image.Should().NotBeNull();
    }

    [Test]
    public void BuildAppRuleMatched_SetsPriorityAndTitle()
    {
        var playback = CreateDevice("Speakers", EDataFlow.eRender);
        var recording = CreateDevice("Mic", EDataFlow.eCapture);
        var pair = new AppSoundRuleDevicePair
        {
            Playback = CreateStoredDevice("Speakers", EDataFlow.eRender),
            Recording = CreateStoredDevice("Mic", EDataFlow.eCapture)
        };

        var data = NotificationContentBuilder.BuildAppRuleMatched("spotify", pair, playback, recording,
            new[] { EDataFlow.eRender, EDataFlow.eCapture }, new Bitmap(1, 1));

        data.Priority.Should().Be(1);
        data.Title.Should().Be(string.Format(SettingsStrings.appSoundLock_rule_switchedApp, "spotify"));
        data.Title.Should().Contain("spotify");
        data.Title.Should().NotContain(SettingsStrings.appSoundLock_tab);
        data.Text.Should().Contain($"{SettingsStrings.playback} → {playback.NameClean}");
        data.Text.Should().Contain($"{SettingsStrings.recording} → {recording.NameClean}");
    }

    [Test]
    public void BuildAppRuleMatched_PlaybackOnlyFlow_ShowsSinglePlaybackLine()
    {
        var playback = CreateDevice("Speakers", EDataFlow.eRender);
        var pair = new AppSoundRuleDevicePair
        {
            Playback = CreateStoredDevice("Speakers", EDataFlow.eRender),
            Recording = CreateStoredDevice("Mic", EDataFlow.eCapture)
        };

        var data = NotificationContentBuilder.BuildAppRuleMatched("spotify", pair, playback, null,
            new[] { EDataFlow.eRender }, new Bitmap(1, 1));

        data.Text.Should().Be($"{SettingsStrings.playback} → {playback.NameClean}");
        data.Text.Should().NotContain(SettingsStrings.recording);
    }

    [Test]
    public void BuildAppRuleMatched_UnresolvedDevice_FallsBackToStoredName()
    {
        var pair = new AppSoundRuleDevicePair
        {
            Playback = CreateStoredDevice("Headset", EDataFlow.eRender),
            Recording = CreateStoredDevice("CABLE Output", EDataFlow.eCapture)
        };

        var data = NotificationContentBuilder.BuildAppRuleMatched("spotify", pair, null, null,
            new[] { EDataFlow.eRender, EDataFlow.eCapture }, new Bitmap(1, 1));

        data.Text.Should().Be($"{SettingsStrings.playback} → {pair.Playback.NameClean}\n{SettingsStrings.recording} → {pair.Recording.NameClean}");
        data.Text.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void BuildAppRuleMatched_CompactContent_JoinsNamesWithMiddot()
    {
        var playback = CreateDevice("Speakers", EDataFlow.eRender);
        var recording = CreateDevice("CABLE Output", EDataFlow.eCapture);
        var pair = new AppSoundRuleDevicePair
        {
            Playback = CreateStoredDevice("Speakers", EDataFlow.eRender),
            Recording = CreateStoredDevice("CABLE Output", EDataFlow.eCapture)
        };

        var data = NotificationContentBuilder.BuildAppRuleMatched("spotify", pair, playback, recording,
            new[] { EDataFlow.eRender, EDataFlow.eCapture }, new Bitmap(1, 1));

        data.CompactContent.Should().NotBeNull();
        data.CompactContent.Title.Should().Be(data.Title);
        data.CompactContent.Text.Should().Be($"{playback.NameClean} · {recording.NameClean}");
    }

    [Test]
    public void BuildMicrophoneMuteChanged_Muted_SetsTitleAndImage()
    {
        var data = NotificationContentBuilder.BuildMicrophoneMuteChanged("Mic", true);

        data.Priority.Should().Be(2);
        data.Title.Should().Be(string.Format(SettingsStrings.notification_microphone_muted, "Mic"));
        data.Image.Should().NotBeNull();
    }

    [Test]
    public void BuildMicrophoneMuteChanged_Unmuted_SetsTitleAndImage()
    {
        var data = NotificationContentBuilder.BuildMicrophoneMuteChanged("Mic", false);

        data.Priority.Should().Be(2);
        data.Title.Should().Be(string.Format(SettingsStrings.notification_microphone_unmuted, "Mic"));
        data.Image.Should().NotBeNull();
    }
}
