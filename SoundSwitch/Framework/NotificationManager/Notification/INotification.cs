using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Factory;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public interface INotification : IEnumImpl<NotificationTypeEnum>
    {
        /// <summary>
        ///     Configuration of the Notification
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
        ///     Does the notification need a Custom Sound set to work
        /// </summary>
        /// <returns></returns>
        bool NeedCustomSound();

        /// <summary>
        ///    Is this notification available
        /// </summary>
        /// <returns></returns>
        bool IsAvailable();
    }
}