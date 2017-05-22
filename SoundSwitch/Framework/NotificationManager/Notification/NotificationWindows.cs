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
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationWindows : INotification
    {
        public NotificationTypeEnum TypeEnum => NotificationTypeEnum.DefaultWindowsNotification;
        public string Label => SettingsStrings.notificationOptionWindowsDefault;

        public INotificationConfiguration Configuration { get; set; }

        public void NotifyDefaultChanged(IAudioDevice audioDevice)
        {
            switch (audioDevice.Type)
            {
                case AudioDeviceType.Playback:
                    Configuration.Icon.ShowBalloonTip(500,
                                                      TrayIconStrings.playbackChanged,
                                                      audioDevice.FriendlyName, ToolTipIcon.Info);
                    break;
                case AudioDeviceType.Recording:
                    Configuration.Icon.ShowBalloonTip(500,
                                                      TrayIconStrings.recordingChanged,
                                                      audioDevice.FriendlyName, ToolTipIcon.Info);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(audioDevice.Type), audioDevice.Type, null);
            }
        }

        public void OnSoundChanged(CachedSound newSound)
        {
        }

        public NotificationCustomSoundEnum SupportCustomSound() => NotificationCustomSoundEnum.NotSupported;

        public bool NeedCustomSound()
        {
            return false;
        }

        public bool IsAvailable()
        {
            return true;
        }
    }
}