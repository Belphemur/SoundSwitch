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
using System.IO;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Localization;
using SoundSwitch.Util;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationBanner : INotification
    {
        public NotificationTypeEnum TypeEnum => NotificationTypeEnum.BannerNotification;
        public string Label => SettingsStrings.notificationOptionBanner;

        public INotificationConfiguration Configuration { get; set; }

        public void NotifyDefaultChanged(IAudioDevice audioDevice)
        {
            var toastData = new BannerData
            {
                Image = AudioDeviceIconExtractor.ExtractIconFromAudioDevice(audioDevice, true).ToBitmap(),
                Text = audioDevice.FriendlyName
            };
            if (Configuration.CustomSound != null && File.Exists(Configuration.CustomSound.FilePath))
            {
                toastData.SoundFilePath = Configuration.CustomSound.FilePath;
            }

            switch (audioDevice.Type)
            {
                case AudioDeviceType.Playback:
                    toastData.Title = SettingsStrings.tooltipOnHoverOptionPlaybackDevice;
                    break;
                case AudioDeviceType.Recording:
                    toastData.Title = SettingsStrings.tooltipOnHoverOptionRecordingDevice;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(audioDevice.Type), audioDevice.Type, null);
            }
            new BannerManager().ShowNotification(toastData);
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
            BannerManager.Setup();

            // Available in all Windows versions
            return true;
        }
    }
}