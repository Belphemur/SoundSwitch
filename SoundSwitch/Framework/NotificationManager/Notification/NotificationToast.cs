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
using SoundSwitch.Framework.Toast;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationToast : INotification
    {
        public NotificationTypeEnum TypeEnum => NotificationTypeEnum.ToastNotification;
        public string Label => SettingsStrings.notificationOptionToast;

        public INotificationConfiguration Configuration { get; set; }

        public void NotifyDefaultChanged(IAudioDevice audioDevice)
        {
            var toastData = new ToastData
            {
                ImagePath = "file:///"+ApplicationPath.DefaultImagePath,
                Title = audioDevice.FriendlyName
            };
            if (Configuration.CustomSound != null && File.Exists(Configuration.CustomSound.FilePath))
            {
                toastData.Silent = false;
                toastData.SoundFilePath = Configuration.CustomSound.FilePath;
            }

            switch (audioDevice.Type)
            {
                case AudioDeviceType.Playback:
                    toastData.Line0 = SettingsStrings.tooltipOnHoverOptionPlaybackDevice;
                    break;
                case AudioDeviceType.Recording:
                    toastData.Line0 = SettingsStrings.tooltipOnHoverOptionRecordingDevice;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(audioDevice.Type), audioDevice.Type, null);
            }
            new ToastManager().ShowNotification(toastData);
        }

        public void OnSoundChanged(CachedSound newSound)
        {
            Configuration.CustomSound = newSound;
        }

        public NotificationCustomSoundEnum SupportCustomSound() => NotificationCustomSoundEnum.NotSupported;

        public bool NeedCustomSound()
        {
            return false;
        }

        public bool IsAvailable()
        {
            // Not available before Windows 8
            return Environment.OSVersion.Version >= new Version(6, 2);
        }
    }
}