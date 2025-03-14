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

using System.Drawing;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    internal class NotificationNone : INotification
    {
        public NotificationTypeEnum TypeEnum => NotificationTypeEnum.NoNotification;
        public string Label => SettingsStrings.none;

        public INotificationConfiguration Configuration { get; set; }

        public void NotifyDefaultChanged(DeviceFullInfo audioDevice) { }

        public void NotifyProfileChanged(Profile.Profile profile, Bitmap icon, uint? processId) { }

        public void NotifyMuteChanged(string deviceId, string microphoneName, bool newMuteState) { }

        public void OnSoundChanged(CachedSound newSound) { }

        public bool SupportCustomSound() => false;

        public bool IsAvailable() => true;
    }
}