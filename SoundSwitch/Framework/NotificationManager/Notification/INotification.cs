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
using JetBrains.Annotations;
using SoundSwitch.Common.Framework.Audio.Device;
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
        void NotifyDefaultChanged(DeviceFullInfo audioDevice);

        /// <summary>
        /// Notify when a profile has changed
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="icon"></param>
        /// <param name="processId"></param>
        void NotifyProfileChanged(Profile.Profile profile, [CanBeNull] Bitmap icon, uint? processId);

        /// <summary>
        /// Notify about the mute state having changed
        /// </summary>
        void NotifyMuteChanged(string deviceId, string microphoneName, bool newMuteState);

        /// <summary>
        /// Does this notification support showing an icon
        /// </summary>
        bool SupportIcon => false;

        /// <summary>
        /// Called when the set sound changed
        /// </summary>
        /// <param name="newSound"></param>
        void OnSoundChanged(CachedSound newSound);

        /// <summary>
        /// Does the notification support a Custom Sound
        /// </summary>
        /// <returns></returns>
        bool SupportCustomSound();

        /// <summary>
        /// Is this notification available
        /// </summary>
        /// <returns></returns>
        bool IsAvailable();
    }
}