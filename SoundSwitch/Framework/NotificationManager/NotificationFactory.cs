using System;
using System.Collections.Generic;

using SoundSwitch.Framework.NotificationManager.Notification;

namespace SoundSwitch.Framework.NotificationManager
{
    public class NotificationFactory
    {
        public static readonly List<INotification> AllNotifications = new List<INotification>
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