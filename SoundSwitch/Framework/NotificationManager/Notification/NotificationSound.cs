using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AudioEndPointControllerWrapper;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.configuration;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationSound : INotification
    {
        private readonly MMDeviceEnumerator _deviceEnumerator;

        public NotificationSound()
        {
            _deviceEnumerator = new MMDeviceEnumerator();
        }

        private Stream GetStreamCopy()
        {
            lock (this)
            {
                Configuration.DefaultSound.Position = 0;
                var memoryStreamedSound = new MemoryStream();
                Configuration.DefaultSound.CopyTo(memoryStreamedSound);
                memoryStreamedSound.Position = 0;
                return memoryStreamedSound;
            }
        }

        public INotificationConfiguration Configuration { get; set; }

        public void NotifyDefaultChanged(IAudioDevice audioDevice)
        {
            if (audioDevice.Type != AudioDeviceType.Playback)
                return;

            var task = new Task(() =>
            {
                using (var memoryStreamedSound = GetStreamCopy())
                {
                    var device = _deviceEnumerator.GetDevice(audioDevice.Id);
                    using (var output = new WasapiOut(device, AudioClientShareMode.Shared, true, 10))
                    {
                        output.Init(new WaveFileReader(memoryStreamedSound));
                        output.Play();
                        while (output.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(500);
                        }
                    }
                }
            });
            task.Start();
        }

        public void OnSoundChanged(CachedSound newSound)
        {
        }

        public NotificationDisplayer Displayer()
        {
            return new NotificationDisplayer(Type(), Properties.Notifications.NotifSound);
        }

        public NotificationTypeEnum Type()
        {
            return NotificationTypeEnum.SoundNotification;
        }

        public bool NeedCustomSound()
        {
            return false;
        }
    }
}