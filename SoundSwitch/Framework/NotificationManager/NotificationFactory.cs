using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification;

namespace SoundSwitch.Framework.NotificationManager
{
    public class NotificationFactory
    {
        /// <summary>
        ///     Create a notification object linked to the enum value
        /// </summary>
        /// <param name="eEnum"></param>
        /// <param name="icon"></param>
        /// <param name="sound"></param>
        /// <returns></returns>
        public static INotification CreateNotification(NotificationTypeEnum eEnum, NotifyIcon icon, CachedSound sound)
        {
            switch (eEnum)
            {
                case NotificationTypeEnum.DefaultWindowsNotification:
                    return new NotificationWindows(icon);
                case NotificationTypeEnum.SoundNotification:
                    return new NotificationSound();
                case NotificationTypeEnum.NoNotification:
                    return new NotificationNone();
                default:
                    throw new ArgumentOutOfRangeException(nameof(eEnum), eEnum, null);
            }
        }
        /// <summary>
        /// Return a diplyable list of Notification
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<NotificationDisplayer> GetNotificationDisplayers()
        {
            var displayers = new List<NotificationDisplayer>
            {
                new NotificationDisplayer(NotificationTypeEnum.DefaultWindowsNotification,
                    Properties.Notifications.NotifWindows),
                new NotificationDisplayer(NotificationTypeEnum.SoundNotification, Properties.Notifications.NotifSound),
                new NotificationDisplayer(NotificationTypeEnum.NoNotification, Properties.Notifications.NotifNone)
            };
            return displayers;
        }
    }
}