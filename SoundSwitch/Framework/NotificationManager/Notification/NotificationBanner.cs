/********************************************************************
 * Copyright (C) 2015-2017 Antoine Aflalo
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
using System.IO;
using System.Linq;
using NAudio.CoreAudioApi;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Framework.Banner.BannerPosition;
using SoundSwitch.Framework.Banner.BannerPosition.Position;
using SoundSwitch.Framework.Banner.MicrophoneMute;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.NotificationManager.Notification;

internal class NotificationBanner : INotification
{
    public NotificationType TypeEnum => NotificationType.BannerNotification;
    public string Label => SettingsStrings.notification_option_banner;

    public INotificationConfiguration Configuration { get; set; }
    private readonly BannerManager _bannerManager = new();
    private readonly BannerPositionFactory _bannerPositionFactory = new();
    private readonly MicrophoneMuteBannerManager _microphoneMuteBannerManager = new();

    private IPosition BannerPosition => _bannerPositionFactory.Get(Configuration.BannerPosition);

    public void NotifyDefaultChanged(DeviceFullInfo audioDevice)
    {
        using var iconHandle = audioDevice.LargeIcon;
        var toastData = new BannerData
        {
            Image = iconHandle.ToBitmap(),
            Text = audioDevice.NameClean,
            Position = BannerPosition,
            Ttl = Configuration.Ttl,
            Opacity = Configuration.Opacity
        };
        if (CustomSoundCheck(audioDevice))
        {
            toastData.SoundFile = Configuration.CustomSound;
            toastData.CurrentDeviceId = audioDevice.Id;
        }

        toastData.Title = audioDevice.Type switch
        {
            DataFlow.Render => SettingsStrings.tooltipOnHover_option_playbackDevice,
            DataFlow.Capture => SettingsStrings.tooltipOnHover_option_recordingDevice,
            _ => throw new ArgumentOutOfRangeException(nameof(audioDevice.Type), audioDevice.Type, null)
        };

        _bannerManager.ShowNotification(toastData);
    }

    public void NotifyProfileChanged(Profile.Profile profile, Bitmap icon, uint? processId)
    {
        var bannerData = new BannerData
        {
            Priority = 1,
            Image = icon,
            Title = string.Format(SettingsStrings.profile_notification_text, profile.Name),
            Text = string.Join("\n", profile.Devices.Select(wrapper => wrapper.DeviceInfo.NameClean).Distinct()),
            Position = BannerPosition,
            Ttl = Configuration.Ttl,
            Opacity = Configuration.Opacity
        };
        _bannerManager.ShowNotification(bannerData);
    }

    public void NotifyAppRuleMatched(AppSoundRule rule, DeviceFullInfo playback, DeviceFullInfo recording, Bitmap icon, uint processId)
    {
        var devices = new List<string>();
        if (playback != null) devices.Add(playback.NameClean);
        if (recording != null) devices.Add(recording.NameClean);

        var bannerData = new BannerData
        {
            Priority = 1,
            Image = icon,
            Title = SettingsStrings.appSoundLock_tab,
            Text = string.Join("\n", devices),
            Position = BannerPosition,
            Ttl = Configuration.Ttl,
            Opacity = Configuration.Opacity
        };
        _bannerManager.ShowNotification(bannerData);
    }

    public void NotifyMicrophoneMuteChanged(string deviceId, string microphoneName, bool newMuteState)
    {
        var microphoneMuteBanner = newMuteState ? Configuration.MicrophoneMuteBanner : Configuration.MicrophoneUnmuteBanner;

        if (Configuration.MicrophoneMuteBanner != MicrophoneMute.Persistent || Configuration.MicrophoneUnmuteBanner != MicrophoneMute.Persistent)
            _microphoneMuteBannerManager.RemovePersistentMuteBanner(deviceId);

        switch (microphoneMuteBanner)
        {
            case MicrophoneMute.Persistent:
                _microphoneMuteBannerManager.UpdateMicrophoneMuteState(deviceId, microphoneName, newMuteState);
                return;
            case MicrophoneMute.Fading:
                FullBanner(microphoneName, newMuteState);
                return;
            case MicrophoneMute.None:
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void FullBanner(string microphoneName, bool newMuteState)
    {
        var title = newMuteState ?
            string.Format(SettingsStrings.notification_microphone_muted, microphoneName) :
            string.Format(SettingsStrings.notification_microphone_unmuted, microphoneName);

        var icon = newMuteState ? Resources.microphone_muted : Resources.microphone_unmuted;

        var bannerData = new BannerData
        {
            Priority = 2,
            Image = icon,
            Title = title,
            Position = BannerPosition,
            Ttl = Configuration.Ttl,
            Opacity = Configuration.Opacity
        };
        _bannerManager.ShowNotification(bannerData);
    }

    public bool SupportIcon => true;

    public void OnSoundChanged(CachedSound newSound) => Configuration.CustomSound = newSound;

    public bool SupportCustomSound() => true;

    // Available in all Windows versions
    public bool IsAvailable() => true;

    public bool CustomSoundCheck(DeviceFullInfo audioDevice) =>
        audioDevice.Type == DataFlow.Render && Configuration.CustomSound != null && File.Exists(Configuration.CustomSound.FilePath);
}
