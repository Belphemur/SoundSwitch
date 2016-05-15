using System;
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationWindows : INotification
    {
        public NotificationTypeEnum TypeEnum { get; } = NotificationTypeEnum.DefaultWindowsNotification;
        public string Label { get; } = Notifications.NotifWindows;

        public INotificationConfiguration Configuration { get; set; }

        public void NotifyDefaultChanged(IAudioDevice audioDevice)
        {
            switch (audioDevice.Type)
            {
                case AudioDeviceType.Playback:
                    Configuration.Icon.ShowBalloonTip(500,
                        string.Format(TrayIconStrings.playbackChanged, Application.ProductName),
                        audioDevice.FriendlyName, ToolTipIcon.Info);
                    break;
                case AudioDeviceType.Recording:
                    Configuration.Icon.ShowBalloonTip(500,
                        string.Format(TrayIconStrings.recordingChanged, Application.ProductName),
                        audioDevice.FriendlyName, ToolTipIcon.Info);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(audioDevice.Type), audioDevice.Type, null);
            }
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