using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification;
using SoundSwitch.Model;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.NotificationManager
{
    public class NotificationFactory
    {
        public static readonly List<INotification> AllNotifications = new List<INotification>()
        {
            new NotificationWindows(),
            new NotificationSound(),
            new NotificationCustom(),
            new NotificationNone()
        };

        /// <summary>
        ///     Create a notification object linked to the enum value
        /// </summary>
        /// <param name="eEnum"></param>
        /// <exception cref="CachedSoundFileNotExistsException">For a CustomNotification if the CustomSound file is not present.</exception>
        /// <returns></returns>
        public static INotification CreateNotification(NotificationTypeEnum eEnum)
        {
            switch (eEnum)
            {
                case NotificationTypeEnum.DefaultWindowsNotification:
                    return new NotificationWindows();
                case NotificationTypeEnum.SoundNotification:
                    return new NotificationSound();
                case NotificationTypeEnum.NoNotification:
                    return new NotificationNone();
                case NotificationTypeEnum.CustomNotification:
                    return new NotificationCustom();
                default:
                    throw new ArgumentOutOfRangeException(nameof(eEnum), eEnum, null);
            }
        }
    }
}