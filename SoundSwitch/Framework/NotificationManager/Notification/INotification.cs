using AudioEndPointControllerWrapper;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public interface INotification
    {
        /// <summary>
        /// Notify the change of default audio device
        /// </summary>
        /// <param name="audioDevice"></param>
        void NotifyDefaultChanged(IAudioDevice audioDevice);
    }
}
