using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.configuration;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public interface INotification
    {
        /// <summary>
        /// Configuration of the Notification
        /// </summary>
        INotificationConfiguration Configuration { get; set; }   
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

        /// <summary>
        /// Used to display the notification
        /// </summary>
        /// <returns></returns>
        NotificationDisplayer Displayer();

        /// <summary>
        /// Type of the notification
        /// </summary>
        /// <returns></returns>
        NotificationTypeEnum Type();

        /// <summary>
        /// Does the notification need a Custom Sound set to work
        /// </summary>
        /// <returns></returns>
        bool NeedCustomSound();
    }
}