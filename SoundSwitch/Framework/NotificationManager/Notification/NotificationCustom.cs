using System.Threading;
using System.Threading.Tasks;
using AudioEndPointControllerWrapper;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationCustom : INotification
    {
        private readonly CachedSound _sound;
        private readonly MMDeviceEnumerator _deviceEnumerator = new MMDeviceEnumerator();

        public NotificationCustom(CachedSound sound)
        {
            _sound = sound;
        }

        public void NotifyDefaultChanged(IAudioDevice audioDevice)
        {
            if (audioDevice.Type != AudioDeviceType.Playback)
                return;
            var task = new Task(() =>
            {
                var device = _deviceEnumerator.GetDevice(audioDevice.Id);
                using (var output = new WasapiOut(device, AudioClientShareMode.Shared, true, 10))
                using(var waveStream  = new CachedSoundWaveStream(_sound))
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
    }
}