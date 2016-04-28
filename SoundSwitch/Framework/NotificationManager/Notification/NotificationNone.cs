using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.configuration;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationNone : INotification
    {
        public INotificationConfiguration Configuration { get; set; }

        public void NotifyDefaultChanged(IAudioDevice audioDevice)
        {
        }

        public void OnSoundChanged(CachedSound newSound)
        {
        }

        public NotificationDisplayer Displayer()
        {
            return new NotificationDisplayer(Type(), Properties.Notifications.NotifNone);
        }

        public NotificationTypeEnum Type()
        {
            return NotificationTypeEnum.NoNotification;
        }

        public bool NeedCustomSound()
        {
            return false;
        }
    }
}