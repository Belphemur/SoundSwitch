using SoundSwitch.Framework.Factory;
using SoundSwitch.Framework.NotificationManager.Notification;

namespace SoundSwitch.Framework.NotificationManager
{
    public class NotificationFactory : AbstractFactory<NotificationTypeEnum, INotification>
    {
        private static readonly IEnumImplList<NotificationTypeEnum, INotification> Notifications = new EnumImplList
            <NotificationTypeEnum, INotification>
        {
            new NotificationWindows(),
            new NotificationSound(),
            new NotificationNone(),
            new NotificationCustom()
        };

        public NotificationFactory() : base(Notifications)
        {
        }
    }
}