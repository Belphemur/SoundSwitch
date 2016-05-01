using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationNone : INotification
    {
        public NotificationTypeEnum TypeEnum { get; } = NotificationTypeEnum.NoNotification;
        public string Label { get; } = Notifications.NotifNone;

        public INotificationConfiguration Configuration { get; set; }

        public void NotifyDefaultChanged(IAudioDevice audioDevice)
        {
        }

        public void OnSoundChanged(CachedSound newSound)
        {
        }

        public bool NeedCustomSound()
        {
            return false;
        }
    }
}