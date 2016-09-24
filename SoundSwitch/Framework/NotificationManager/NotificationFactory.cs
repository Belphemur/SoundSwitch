using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
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
                new NotificationToast(),
                new NotificationCustom()
            };

        public NotificationFactory() : base(Notifications)
        {
        }


        /// <summary>
        ///     Get the implementation for the given Enum
        /// </summary>
        /// <param name="eEnum"></param>
        /// <returns></returns>
        public new INotification Get(NotificationTypeEnum eEnum)
        {
            INotification value;
            if (!AllImplementations.TryGetValue(eEnum, out value))
                throw new InvalidEnumArgumentException();
            if (!value.IsAvailable())
                throw new InvalidEnumArgumentException(@"Can't be selected");
            return value;
        }

        /// <summary>
        ///     Configure the list control DataSource, ValueMember and DisplayMember
        /// </summary>
        /// <param name="list"></param>
        public new void ConfigureListControl(ListControl list)
        {
            list.DataSource =
                AllImplementations.Values.Where(notif => notif.IsAvailable()).Select(
                        implementation => new {Type = implementation.TypeEnum, Display = implementation.Label})
                    .ToArray();
            list.ValueMember = "Type";
            list.DisplayMember = "Display";
        }
    }
}