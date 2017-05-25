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
using SoundSwitch.Framework.Factory;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public interface INotification : IEnumImpl<NotificationTypeEnum>
    {
        /// <summary>
        /// Configuration of the Notification
        /// </summary>
        INotificationConfiguration Configuration { get; set; }

        /// <summary>
        /// Notify the change of default audio device
        /// </summary>
        /// <param name="audioDevice"></param>
        void NotifyDefaultChanged(IAudioDevice audioDevice);

        /// <summary>
        /// Called when the set sound changed
        /// </summary>
        /// <param name="newSound"></param>
        void OnSoundChanged(CachedSound newSound);

        /// <summary>
        /// Does the notification support a Custom Sound
        /// </summary>
        /// <returns></returns>
        NotificationCustomSoundEnum SupportCustomSound();

        /// <summary>
        /// Does the notification need a Custom Sound set to work
        /// </summary>
        /// <returns></returns>
        bool NeedCustomSound();

        /// <summary>
        /// Is this notification available
        /// </summary>
        /// <returns></returns>
        bool IsAvailable();
    }
}