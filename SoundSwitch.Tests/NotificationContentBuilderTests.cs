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
using System.Drawing;

using NAudio.CoreAudioApi;

using NUnit.Framework;
using FluentAssertions;

using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.NotificationManager.Notification;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Localization;

namespace SoundSwitch.Tests;

[TestFixture]
public class NotificationContentBuilderTests
{
    private static DeviceFullInfo CreateDevice(string name, DataFlow type) =>
        new(name, $"id-{name}", type, @"C:\nonexistent\device.ico", DeviceState.Active, false);

    [Test]
    public void BuildDefaultChanged_RenderWindowsNotification_SetsTitleTextAndImage()
    {
        var device = CreateDevice("Test Speaker", DataFlow.Render);

        var data = NotificationContentBuilder.BuildDefaultChanged(device, NotificationContentBuilder.DeviceChangeWording.WindowsNotification);

        data.Title.Should().Be(TrayIconStrings.playbackChanged);
        data.Text.Should().Be(device.NameClean);
        data.Image.Should().NotBeNull();
    }

    [Test]
    public void BuildDefaultChanged_RenderBanner_SetsPlaybackDeviceTitle()
    {
        var device = CreateDevice("Test Speaker", DataFlow.Render);

        var data = NotificationContentBuilder.BuildDefaultChanged(device, NotificationContentBuilder.DeviceChangeWording.Banner);

        data.Title.Should().Be(SettingsStrings.tooltipOnHover_option_playbackDevice);
    }

    [Test]
    public void BuildDefaultChanged_CaptureWindowsNotification_SetsRecordingTitle()
    {
        var device = CreateDevice("Test Microphone", DataFlow.Capture);

        var data = NotificationContentBuilder.BuildDefaultChanged(device, NotificationContentBuilder.DeviceChangeWording.WindowsNotification);

        data.Title.Should().Be(TrayIconStrings.recordingChanged);
        data.Text.Should().Be(device.NameClean);
    }

    [Test]
    public void BuildDefaultChanged_CaptureBanner_SetsRecordingDeviceTitle()
    {
        var device = CreateDevice("Test Microphone", DataFlow.Capture);

        var data = NotificationContentBuilder.BuildDefaultChanged(device, NotificationContentBuilder.DeviceChangeWording.Banner);

        data.Title.Should().Be(SettingsStrings.tooltipOnHover_option_recordingDevice);
    }

    [Test]
    public void BuildProfileChanged_SetsPriorityTitleAndText()
    {
        var profile = new Profile
        {
            Name = "Gaming",
            Playback = new DeviceInfo("Speakers", "id-speakers", DataFlow.Render, false, DateTime.UtcNow),
            Recording = new DeviceInfo("Mic", "id-mic", DataFlow.Capture, false, DateTime.UtcNow)
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
        var playback = CreateDevice("Speakers", DataFlow.Render);
        var recording = CreateDevice("Mic", DataFlow.Capture);

        var data = NotificationContentBuilder.BuildAppRuleMatched(playback, recording, new Bitmap(1, 1));

        data.Priority.Should().Be(1);
        data.Title.Should().Be(SettingsStrings.appSoundLock_tab);
        data.Text.Should().Contain(playback.NameClean);
        data.Text.Should().Contain(recording.NameClean);
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
