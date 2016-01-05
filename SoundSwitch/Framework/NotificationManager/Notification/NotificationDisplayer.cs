namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationDisplayer
    {
        public NotificationDisplayer(NotificationTypeEnum notificationTypeEnum, string label)
        {
            Type = notificationTypeEnum;
            Label = label;
        }

        public NotificationTypeEnum Type { get; }
        public string Label { get; }

        public override string ToString()
        {
            return Label;
        }
    }
}