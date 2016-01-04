using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AudioEndPointControllerWrapper;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SoundSwitch.Framework.Audio;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationSound : INotification
    {
        private readonly MMDeviceEnumerator _deviceEnumerator;
        private readonly Stream _memoryStreamSound;

        public NotificationSound(Stream memoryStreamSound)
        {
            _memoryStreamSound = memoryStreamSound;
            _deviceEnumerator = new MMDeviceEnumerator();
        }

        public void NotifyDefaultChanged(IAudioDevice audioDevice)
        {
            if (audioDevice.Type != AudioDeviceType.Playback)
                return;
            var task = new Task(() =>
            {
                var device = _deviceEnumerator.GetDevice(audioDevice.Id);
                using (var output = new WasapiOut(device, AudioClientShareMode.Shared, true, 10))
                {
                    output.Init(new WaveFileReader(_memoryStreamSound));
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
        }
    }
}