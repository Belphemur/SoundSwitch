using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Audio;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationNone : INotification
    {
        public void NotifyDefaultChanged(IAudioDevice audioDevice)
        {
        }

        public void OnSoundChanged(CachedSound newSound)
        {
        }
    }
}