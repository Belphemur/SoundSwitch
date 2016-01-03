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
        /// <summary>
        ///     Create a notification object linked to the enum value
        /// </summary>
        /// <param name="eEnum"></param>
        /// <exception cref="CachedSoundFileNotExistsException">For a CustomNotification if the Sound file is not present.</exception>
        /// <returns></returns>
        public static INotification CreateNotification(NotificationTypeEnum eEnum)
        {
            switch (eEnum)
            {
                case NotificationTypeEnum.DefaultWindowsNotification:
                    return new NotificationWindows(AppModel.Instance.NotifyIcon);
                case NotificationTypeEnum.SoundNotification:
                    return new NotificationSound(Resources.NotificationSound);
                case NotificationTypeEnum.NoNotification:
                    return new NotificationNone();
                case NotificationTypeEnum.CustomNotification:
                    return new NotificationCustom(AppModel.Instance.CustomNotificationSound);
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
                new NotificationDisplayer(NotificationTypeEnum.CustomNotification, Properties.Notifications.NotifCustom),
                new NotificationDisplayer(NotificationTypeEnum.NoNotification, Properties.Notifications.NotifNone)
            };
            return displayers;
        }
    }
}