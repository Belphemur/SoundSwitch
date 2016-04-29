using System.Threading;
using System.Threading.Tasks;
using AudioEndPointControllerWrapper;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.configuration;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationCustom : INotification
    {
        private readonly MMDeviceEnumerator _deviceEnumerator = new MMDeviceEnumerator();
        public NotificationTypeEnum TypeEnum { get; } = NotificationTypeEnum.CustomNotification;
        public string Label { get; } = Notifications.NotifCustom;


        public INotificationConfiguration Configuration { get; set; }

        public void NotifyDefaultChanged(IAudioDevice audioDevice)
        {
            if (audioDevice.Type != AudioDeviceType.Playback)
                return;
            var task = new Task(() =>
            {
                var device = _deviceEnumerator.GetDevice(audioDevice.Id);
                using (var output = new WasapiOut(device, AudioClientShareMode.Shared, true, 10))
                using (var waveStream = new CachedSoundWaveStream(Configuration.CustomSound))
                {
                    output.Init(waveStream);
                    output.Play();
                    while (output.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(500);
                    }
                }
            });
            task.Start();
        }

        public void OnSoundChanged(CachedSound newSound)
        {
            Configuration.CustomSound = newSound;
        }

        public bool NeedCustomSound()
        {
            return true;
        }
    }
}