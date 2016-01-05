using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Audio;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public interface INotification
    {
        /// <summary>
        ///     Notify the change of default audio device
        /// </summary>
        /// <param name="audioDevice"></param>
        void NotifyDefaultChanged(IAudioDevice audioDevice);

        /// <summary>
        ///     Called when the set sound changed
        /// </summary>
        /// <param name="newSound"></param>
        void OnSoundChanged(CachedSound newSound);
    }
}