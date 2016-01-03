using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Wave;

namespace SoundSwitch.Framework.Audio
{
    public class CachedSound
    {
        public byte[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }
        public string FilePath { get; }

        /// <summary>
        /// Load the AudioFile into the memory using the right reader.
        /// </summary>
        /// <param name="audioFileName"></param>
        /// <exception cref="ArgumentException">Audio file doesn't exists</exception>
        public CachedSound(string audioFileName)
        {
            if (!File.Exists(audioFileName))
            {
                throw new CachedSoundFileNotExistsException("The audio file doesn't exists");
            }
            FilePath = audioFileName;
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {
                // TODO: could add resampling in here if required
                WaveFormat = audioFileReader.WaveFormat;
                var wholeFile = new List<byte>((int) (audioFileReader.Length));
                var readBuffer = new byte[audioFileReader.WaveFormat.SampleRate*audioFileReader.WaveFormat.Channels];
                int samplesRead;
                while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                }
                AudioData = wholeFile.ToArray();
            }
        }
    }
}