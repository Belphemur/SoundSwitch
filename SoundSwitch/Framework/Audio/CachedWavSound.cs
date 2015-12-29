using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace SoundSwitch.Framework.Audio
{
    public class CachedWavSound
    {
        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }
        public CachedWavSound(Stream audioStream)
        {
            using (var audioFileReader = GetAudioReader(audioStream))
            {
                var outFormat = new WaveFormat(44100, audioFileReader.WaveFormat.Channels);
                using (var resampler = new MediaFoundationResampler(audioFileReader, outFormat))
                {
                    var sampleChannel = new SampleChannel(resampler, false);
                    // TODO: could add resampling in here if required
                    WaveFormat = sampleChannel.WaveFormat;
                    var wholeFile = new List<float>((int) (audioFileReader.Length/4));
                    var readBuffer = new float[sampleChannel.WaveFormat.SampleRate*audioFileReader.WaveFormat.Channels];
                    int samplesRead;
                    while ((samplesRead = sampleChannel.Read(readBuffer, 0, readBuffer.Length)) > 0)
                    {
                        wholeFile.AddRange(readBuffer.Take(samplesRead));
                    }
                    AudioData = wholeFile.ToArray();
                }
            }
        }

        private WaveStream GetAudioReader(Stream audioStream)
        {
            WaveStream readerStream = new WaveFileReader(audioStream);
            if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
            {
                readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                readerStream = new BlockAlignReductionStream(readerStream);
            }
            return readerStream;
        }
    }
}