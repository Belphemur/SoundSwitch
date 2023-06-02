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
using System.Drawing;
using System.IO;
using System.Linq;
using CoreAudio;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Localization;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationBanner : INotification
    {
        public NotificationTypeEnum TypeEnum => NotificationTypeEnum.BannerNotification;
        public string Label => SettingsStrings.notificationOptionBanner;

        public INotificationConfiguration Configuration { get; set; }

        private readonly BannerManager _bannerManager = new();

        public bool SupportIcon => true;

        public void NotifyProfileChanged(Profile.Profile profile, Bitmap icon, uint? processId)
        {
            var bannerData = new BannerData
            {
                Priority = 1,
                Image = icon,
                Title = string.Format(SettingsStrings.profile_notification_text, profile.Name),
                Text = string.Join("\n", profile.Devices.Select(wrapper => wrapper.DeviceInfo.NameClean).Distinct())
            };
            _bannerManager.ShowNotification(bannerData);
        }

        public void NotifyMuteChanged(string microphoneName, bool newMuteState)
        {
            var title = newMuteState ? string.Format(SettingsStrings.notification_microphone_muted, microphoneName) : string.Format(SettingsStrings.notification_microphone_unmuted, microphoneName);

            var icon = newMuteState ? Resources.microphone_muted : Resources.microphone_unmuted;

            var bannerData = new BannerData
            {
                Priority = 2,
                Image = icon,
                Title = title
            };
            _bannerManager.ShowNotification(bannerData);
        }

        public void NotifyDefaultChanged(DeviceFullInfo audioDevice)
        {
            var toastData = new BannerData
            {
                Image = audioDevice.LargeIcon.ToBitmap(),
                Text = audioDevice.NameClean
            };
            if (audioDevice.Type == DataFlow.Render && Configuration.CustomSound != null && File.Exists(Configuration.CustomSound.FilePath))
            {
                toastData.SoundFile = Configuration.CustomSound;
                toastData.CurrentDeviceId = audioDevice.Id;
            }

            toastData.Title = audioDevice.Type switch
            {
                DataFlow.Render  => SettingsStrings.tooltipOnHoverOptionPlaybackDevice,
                DataFlow.Capture => SettingsStrings.tooltipOnHoverOptionRecordingDevice,
                _                => throw new ArgumentOutOfRangeException(nameof(audioDevice.Type), audioDevice.Type, null)
            };

            _bannerManager.ShowNotification(toastData);
        }

        public void OnSoundChanged(CachedSound newSound)
        {
            Configuration.CustomSound = newSound;
        }

        public NotificationCustomSoundEnum SupportCustomSound() => NotificationCustomSoundEnum.Optional;

        public bool NeedCustomSound()
        {
            return false;
        }

        public bool IsAvailable()
        {
            // Available in all Windows versions
            return true;
        }
    }
}