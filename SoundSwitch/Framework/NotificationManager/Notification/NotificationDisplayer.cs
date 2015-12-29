using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationDisplayer
    {
        public NotificationTypeEnum Type { get; }
        public string Label { get;  }

        public NotificationDisplayer(NotificationTypeEnum notificationTypeEnum, string label)
        {
            Type = notificationTypeEnum;
            Label = label;
        }

        public override string ToString()
        {
            return Label;
        }
    }
}
