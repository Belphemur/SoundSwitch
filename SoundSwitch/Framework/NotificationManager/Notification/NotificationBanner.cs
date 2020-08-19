﻿/********************************************************************
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
using NAudio.CoreAudioApi;
using SoundSwitch.Common.Framework.Audio.Icon;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationBanner : INotification
    {
        public NotificationTypeEnum TypeEnum => NotificationTypeEnum.BannerNotification;
        public string Label => SettingsStrings.notificationOptionBanner;

        public INotificationConfiguration Configuration { get; set; }

        public void NotifyDefaultChanged(MMDevice audioDevice)
        {
            var icon = AudioDeviceIconExtractor.ExtractIconFromAudioDevice(audioDevice, true);
            var toastData = new BannerData
            {
                Image = icon.ToBitmap(),
                Text = audioDevice.FriendlyName
            };
            if (Configuration.CustomSound != null && File.Exists(Configuration.CustomSound.FilePath))
            {
                toastData.SoundFile = Configuration.CustomSound;
            }

            switch (audioDevice.DataFlow)
            {
                case DataFlow.Render:
                    toastData.Title = SettingsStrings.tooltipOnHoverOptionPlaybackDevice;
                    break;
                case DataFlow.Capture:
                    toastData.Title = SettingsStrings.tooltipOnHoverOptionRecordingDevice;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(audioDevice.DataFlow), audioDevice.DataFlow, null);
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
            // Available in all Windows versions
            return true;
        }
    }
}