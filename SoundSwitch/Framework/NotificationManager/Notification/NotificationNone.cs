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

using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Localization;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationNone : INotification
    {
        public NotificationTypeEnum TypeEnum => NotificationTypeEnum.NoNotification;
        public string Label => SettingsStrings.notificationOptionNone;

        public INotificationConfiguration Configuration { get; set; }

        public void NotifyDefaultChanged(IAudioDevice audioDevice)
        {
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