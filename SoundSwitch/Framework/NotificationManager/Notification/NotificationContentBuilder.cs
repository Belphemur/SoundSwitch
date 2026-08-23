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
using System.Linq;

using NAudio.CoreAudioApi;

using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Localization;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.NotificationManager.Notification;

/// <summary>
/// Pure, platform-neutral, stateless composer for notification content.
/// Builds the <see cref="BannerData"/> (Title/Text/Image/Priority) shared by the
/// banner and Windows notification channels. Position/Ttl/Opacity/DisplayInfo are
/// intentionally left to the caller.
/// </summary>
internal static class NotificationContentBuilder
{
    public enum DeviceChangeWording
    {
        Banner,
        WindowsNotification
    }

    internal static BannerData BuildDefaultChanged(DeviceFullInfo device, DeviceChangeWording wording)
    {
        using var largeIcon = device.LargeIcon;

        var title = wording switch
        {
            DeviceChangeWording.Banner => device.Type switch
            {
                DataFlow.Render => SettingsStrings.tooltipOnHover_option_playbackDevice,
                DataFlow.Capture => SettingsStrings.tooltipOnHover_option_recordingDevice,
                _ => throw new ArgumentOutOfRangeException(nameof(device.Type), device.Type, null)
            },
            DeviceChangeWording.WindowsNotification => device.Type switch
            {
                DataFlow.Render => TrayIconStrings.playbackChanged,
                DataFlow.Capture => TrayIconStrings.recordingChanged,
                _ => throw new ArgumentOutOfRangeException(nameof(device.Type), device.Type, null)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(wording), wording, null)
        };

        return new BannerData
        {
            Title = title,
            Text = device.NameClean,
            Image = largeIcon.ToBitmap()
        };
    }

    internal static BannerData BuildProfileChanged(Profile.Profile profile, Bitmap icon) => new()
    {
        Priority = 1,
        Image = icon,
        Title = string.Format(SettingsStrings.profile_notification_text, profile.Name),
        Text = string.Join("\n", profile.Devices.Select(wrapper => wrapper.DeviceInfo.NameClean).Distinct())
    };

    internal static BannerData BuildAppRuleMatched(DeviceFullInfo playback, DeviceFullInfo recording, Bitmap icon)
    {
        var devices = new List<string>();
        if (playback != null) devices.Add(playback.NameClean);
        if (recording != null) devices.Add(recording.NameClean);

        return new BannerData
        {
            Priority = 1,
            Image = icon,
            Title = SettingsStrings.appSoundLock_tab,
            Text = string.Join("\n", devices)
        };
    }

    internal static BannerData BuildMicrophoneMuteChanged(string microphoneName, bool newMuteState) => new()
    {
        Priority = 2,
        Title = newMuteState
            ? string.Format(SettingsStrings.notification_microphone_muted, microphoneName)
            : string.Format(SettingsStrings.notification_microphone_unmuted, microphoneName),
        Text = microphoneName,
        Image = newMuteState ? Resources.microphone_muted : Resources.microphone_unmuted
    };
}
