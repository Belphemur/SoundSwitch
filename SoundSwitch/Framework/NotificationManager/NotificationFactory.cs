using System.Collections.Generic;
using System.ComponentModel;
using SoundSwitch.Framework.NotificationManager.Notification;

namespace SoundSwitch.Framework.NotificationManager
{
    public class NotificationFactory
    {
        public static readonly Dictionary<NotificationTypeEnum, INotification> AllNotifications = new Dictionary
            <NotificationTypeEnum, INotification>
        {
            {NotificationTypeEnum.DefaultWindowsNotification, new NotificationWindows()},
            {NotificationTypeEnum.SoundNotification, new NotificationSound()},
            {NotificationTypeEnum.NoNotification, new NotificationNone()},
            {NotificationTypeEnum.CustomNotification, new NotificationCustom()}
        };

        /// <summary>
        ///     Create a notification object linked to the enum value
        /// </summary>
        /// <param name="eEnum"></param>
        /// <returns></returns>
        public static INotification CreateNotification(NotificationTypeEnum eEnum)
        {
            INotification notif;
            if (!AllNotifications.TryGetValue(eEnum, out notif))
            {
                throw new InvalidEnumArgumentException();
            }
            return notif;
        }
    }
}