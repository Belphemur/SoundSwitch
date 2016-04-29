using System.Collections.Generic;
using System.Collections.ObjectModel;
using SoundSwitch.Framework.Factory;
using SoundSwitch.Framework.NotificationManager.Notification;

namespace SoundSwitch.Framework.NotificationManager
{
    public class NotificationFactory : AbstractFactory<NotificationTypeEnum, INotification>
    {
        private static readonly IDictionary<NotificationTypeEnum, INotification> Notifications = new Dictionary
            <NotificationTypeEnum, INotification>
        {
            {NotificationTypeEnum.DefaultWindowsNotification, new NotificationWindows()},
            {NotificationTypeEnum.SoundNotification, new NotificationSound()},
            {NotificationTypeEnum.NoNotification, new NotificationNone()},
            {NotificationTypeEnum.CustomNotification, new NotificationCustom()}
        };

        public NotificationFactory() : base(Notifications)
        {
        }
    }
}