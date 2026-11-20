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

using SoundSwitch.Audio.Manager.Interop.Enum;

using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Localization;
using SoundSwitch.Model;
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
                EDataFlow.eRender => SettingsStrings.tooltipOnHover_option_playbackDevice,
                EDataFlow.eCapture => SettingsStrings.tooltipOnHover_option_recordingDevice,
                _ => throw new ArgumentOutOfRangeException(nameof(device.Type), device.Type, null)
            },
            DeviceChangeWording.WindowsNotification => device.Type switch
            {
                EDataFlow.eRender => TrayIconStrings.playbackChanged,
                EDataFlow.eCapture => TrayIconStrings.recordingChanged,
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

    internal static BannerData BuildAppRuleMatched(string processName, AppSoundRuleDevicePair pair, DeviceFullInfo playback, DeviceFullInfo recording, IReadOnlyCollection<EDataFlow> flowsWritten, Bitmap icon)
    {
        // Resolution can legitimately fail (e.g. device unplugged); the pair's stored name keeps
        // the notification informative instead of empty.
        var playbackName = playback?.NameClean ?? pair.Playback?.NameClean;
        var recordingName = recording?.NameClean ?? pair.Recording?.NameClean;

        // One line per flow actually written, so the text never advertises a role that didn't move.
        var lines = new List<string>();
        var compactNames = new List<string>();
        foreach (var flow in flowsWritten)
        {
            switch (flow)
            {
                case EDataFlow.eRender:
                    lines.Add($"{SettingsStrings.playback} → {playbackName}");
                    compactNames.Add(playbackName);
                    break;
                case EDataFlow.eCapture:
                    lines.Add($"{SettingsStrings.recording} → {recordingName}");
                    compactNames.Add(recordingName);
                    break;
            }
        }

        var title = string.Format(SettingsStrings.appSoundLock_rule_switchedApp, processName);

        return new BannerData
        {
            Priority = 1,
            Image = icon,
            Title = title,
            Text = string.Join("\n", lines),
            // Compact banners drop the role labels: device names only, middot-joined.
            CompactContent = new BannerContent(title, string.Join(" · ", compactNames), icon)
        };
    }

    internal static BannerData BuildAppRuleNoProcess() => new()
    {
        Priority = 1,
        Image = Resources.default_profile_image,
        Title = SettingsStrings.appSoundLock_tab,
        Text = SettingsStrings.appSoundLock_rule_noProcess
    };

    /// <summary>Both microphone presentations are available for either lifetime.</summary>
    internal static BannerData BuildMicrophoneBanner(string microphoneName, bool isMuted)
    {
        var data = BuildMicrophoneMuteChanged(microphoneName, isMuted);
        // The original fading banner put the microphone name in its title only.
        data.Text = null;
        data.CompactContent = new BannerContent(
            isMuted ? SettingsStrings.microphone_off : SettingsStrings.microphone_on,
            microphoneName,
            isMuted ? Resources.MicrophoneOff : Resources.MicrophoneOn);
        return data;
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
