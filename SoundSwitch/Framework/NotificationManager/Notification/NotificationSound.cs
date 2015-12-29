using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Audio;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationSound : INotification
    {
        private readonly CachedWavSound _sound;

        public NotificationSound(CachedWavSound sound)
        {
            _sound = sound;
        }

        public void NotifyDefaultChanged(IAudioDevice audioDevice)
        {
            if (audioDevice.Type != AudioDeviceType.Playback)
                return;
            new AudioPlaybackEngine().PlaySoundWav(_sound);
        }
    }
}
