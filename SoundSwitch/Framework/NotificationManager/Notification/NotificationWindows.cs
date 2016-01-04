using System;
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationWindows : INotification
    {
        private readonly NotifyIcon _notifyIcon;

        public NotificationWindows(NotifyIcon notifyIcon)
        {
            _notifyIcon = notifyIcon;
        }

        public void NotifyDefaultChanged(IAudioDevice audioDevice)
        {
            switch (audioDevice.Type)
            {
                case AudioDeviceType.Playback:
                    _notifyIcon.ShowBalloonTip(500,
                        string.Format(TrayIconStrings.playbackChanged, Application.ProductName),
                        audioDevice.FriendlyName, ToolTipIcon.Info);
                    break;
                case AudioDeviceType.Recording:
                    _notifyIcon.ShowBalloonTip(500,
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
    }
}