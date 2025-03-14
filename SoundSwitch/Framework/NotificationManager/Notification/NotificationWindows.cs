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
using System.Linq;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    internal class NotificationWindows : INotification
    {
        public NotificationTypeEnum TypeEnum => NotificationTypeEnum.DefaultWindowsNotification;
        public string Label => SettingsStrings.notification_option_windowsDefault;

        public INotificationConfiguration Configuration { get; set; }

        public void NotifyDefaultChanged(DeviceFullInfo audioDevice)
        {
            switch (audioDevice.Type)
            {
                case DataFlow.Render:
                    Configuration.Icon.ShowBalloonTip(500,
                        TrayIconStrings.playbackChanged,
                        audioDevice.NameClean, ToolTipIcon.Info);
                    break;
                case DataFlow.Capture:
                    Configuration.Icon.ShowBalloonTip(500,
                        TrayIconStrings.recordingChanged,
                        audioDevice.NameClean, ToolTipIcon.Info);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(audioDevice.Type), audioDevice.Type, null);
            }
        }

        public void NotifyProfileChanged(Profile.Profile profile, Bitmap icon, uint? processId)
        {
            var title = string.Format(SettingsStrings.profile_notification_text, profile.Name);
            var text  = string.Join("\n", profile.Devices.Select(wrapper => wrapper.DeviceInfo.NameClean));
            Configuration.Icon.ShowBalloonTip(1000, title, text, ToolTipIcon.Info);
        }

        public void NotifyMuteChanged(string deviceId, string microphoneName, bool newMuteState)
        {
            var title = newMuteState ? string.Format(SettingsStrings.notification_microphone_muted, microphoneName) : string.Format(SettingsStrings.notification_microphone_unmuted, microphoneName);
            Configuration.Icon.ShowBalloonTip(1000, title, microphoneName, ToolTipIcon.Info);
        }

        public void OnSoundChanged(CachedSound newSound) { }

        public bool SupportCustomSound() => false;

        public bool IsAvailable() => true;
    }
}