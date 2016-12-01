using System;
using System.IO;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Framework.Toast;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationToast : INotification
    {
        public NotificationTypeEnum TypeEnum { get; } = NotificationTypeEnum.ToastNotification;
        public string Label { get; } = Notifications.NotifToast;

        public INotificationConfiguration Configuration { get; set; }

        public void NotifyDefaultChanged(IAudioDevice audioDevice)
        {
            var toastData = new ToastData
            {
                ImagePath = "file:///"+ApplicationPath.DefaultImagePath,
                Title = audioDevice.FriendlyName
            };
            if ((Configuration.CustomSound != null) && File.Exists(Configuration.CustomSound.FilePath))
            {
                toastData.Silent = false;
                toastData.SoundFilePath = Configuration.CustomSound.FilePath;
            }

            switch (audioDevice.Type)
            {
                case AudioDeviceType.Playback:
                    toastData.Line0 = Notifications.PlaybackChanged;

                    break;
                case AudioDeviceType.Recording:
                    toastData.Line0 = Notifications.RecordingChanged;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(audioDevice.Type), audioDevice.Type, null);
            }
            new ToastManager().ShowNotification(toastData);
        }

        public void OnSoundChanged(CachedSound newSound)
        {
            Configuration.CustomSound = newSound;
        }

        public bool NeedCustomSound()
        {
            return false;
        }

        public bool IsAvailable()
        {
            //Not available before windows 8
            return Environment.OSVersion.Version >= new Version(6, 2);
        }
    }
}