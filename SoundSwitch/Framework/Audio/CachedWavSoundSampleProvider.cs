using System;
using NAudio.Wave;

namespace SoundSwitch.Framework.Audio
{
    public class CachedWavSoundSampleProvider : ISampleProvider
    {
        private readonly CachedWavSound _cachedWavSound;
        private long _position;

        public CachedWavSoundSampleProvider(CachedWavSound cachedWavSound)
        {
            this._cachedWavSound = cachedWavSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = _cachedWavSound.AudioData.Length - _position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(_cachedWavSound.AudioData, _position, buffer, offset, samplesToCopy);
            _position += samplesToCopy;
            return (int)samplesToCopy;
        }

        public WaveFormat WaveFormat => _cachedWavSound.WaveFormat;
    }
}