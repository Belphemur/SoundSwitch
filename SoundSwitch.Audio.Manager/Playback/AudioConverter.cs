#nullable enable
using System;
using System.IO;

namespace SoundSwitch.Audio.Manager.Playback
{
    /// <summary>
    /// PCM/IEEE-float sample conversion — the in-house replacement for the format negotiation the
    /// legacy third-party renderer delegated to a Media Foundation resampler. WASAPI shared mode
    /// renders in the engine mix format (typically 48 kHz / 32-bit float) while notification WAVs
    /// are typically 44.1 kHz / 16-bit PCM, so playback almost always needs both a bit-depth /
    /// encoding conversion and a resample.
    ///
    /// The resampler is a plain linear interpolator and channel mapping is the simple
    /// duplicate/average/copy-min scheme — adequate for short notification beeps, and deliberately
    /// not a general-purpose DSP pipeline.
    /// </summary>
    internal static class AudioConverter
    {
        /// <summary>
        /// Convert <paramref name="source"/> from <paramref name="sourceFormat"/> to
        /// <paramref name="targetFormat"/>. The output length is always a whole number of target
        /// sample frames.
        /// </summary>
        public static byte[] Convert(byte[] source, WaveFormat sourceFormat, WaveFormat targetFormat)
        {
            if (sourceFormat == targetFormat) return source;

            var samples = Decode(source, sourceFormat);
            var frames = samples.Length / sourceFormat.Channels;
            samples = MapChannels(samples, frames, sourceFormat.Channels, targetFormat.Channels);
            samples = Resample(samples, frames, sourceFormat.SampleRate, targetFormat.SampleRate, targetFormat.Channels);
            return Encode(samples, targetFormat);
        }

        /// <summary>Decode interleaved samples to normalized floats in [-1, 1].</summary>
        private static float[] Decode(byte[] data, WaveFormat format)
        {
            var bytesPerSample = format.BitsPerSample / 8;
            var samples = new float[data.Length / bytesPerSample];
            for (var i = 0; i < samples.Length; i++)
            {
                var offset = i * bytesPerSample;
                samples[i] = format switch
                {
                    { Encoding: WaveFormatEncoding.Pcm, BitsPerSample: 16 } => (short)(data[offset] | (data[offset + 1] << 8)) / 32768f,
                    { Encoding: WaveFormatEncoding.Pcm, BitsPerSample: 24 } => SignExtend24(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16)) / 8388608f,
                    { Encoding: WaveFormatEncoding.Pcm, BitsPerSample: 32 } => BitConverter.ToInt32(data, offset) / 2147483648f,
                    { Encoding: WaveFormatEncoding.IeeeFloat, BitsPerSample: 32 } => BitConverter.ToSingle(data, offset),
                    _ => throw new InvalidDataException($"Unsupported source format: {format}.")
                };
            }

            return samples;
        }

        private static int SignExtend24(int value) => (value & 0x800000) != 0 ? value | ~0xFFFFFF : value;

        private static float[] MapChannels(float[] samples, int frames, int sourceChannels, int targetChannels)
        {
            if (sourceChannels == targetChannels) return samples;

            var mapped = new float[frames * targetChannels];
            for (var frame = 0; frame < frames; frame++)
            {
                if (targetChannels == 1)
                {
                    // Downmix: average all source channels.
                    float sum = 0;
                    for (var c = 0; c < sourceChannels; c++) sum += samples[frame * sourceChannels + c];
                    mapped[frame] = sum / sourceChannels;
                    continue;
                }

                if (sourceChannels == 1)
                {
                    // Upmix mono: duplicate to every target channel.
                    for (var c = 0; c < targetChannels; c++) mapped[frame * targetChannels + c] = samples[frame];
                    continue;
                }

                // General case: copy the shared channels, silence the rest.
                for (var c = 0; c < targetChannels; c++)
                    mapped[frame * targetChannels + c] = c < sourceChannels ? samples[frame * sourceChannels + c] : 0f;
            }

            return mapped;
        }

        /// <summary>Linear-interpolation resampler; endpoint-preserving.</summary>
        private static float[] Resample(float[] samples, int sourceFrames, int sourceRate, int targetRate, int channels)
        {
            if (sourceRate == targetRate || sourceFrames == 0) return samples;

            var targetFrames = Math.Max(1, (int)((long)sourceFrames * targetRate / sourceRate));
            var resampled = new float[targetFrames * channels];
            var step = targetFrames > 1 ? (double)(sourceFrames - 1) / (targetFrames - 1) : 0;

            for (var frame = 0; frame < targetFrames; frame++)
            {
                var position = frame * step;
                var index = (int)position;
                var fraction = (float)(position - index);
                var next = Math.Min(index + 1, sourceFrames - 1);
                for (var c = 0; c < channels; c++)
                {
                    var a = samples[index * channels + c];
                    var b = samples[next * channels + c];
                    resampled[frame * channels + c] = a + (b - a) * fraction;
                }
            }

            return resampled;
        }

        /// <summary>Encode normalized floats back to interleaved samples, clamped to [-1, 1].</summary>
        private static byte[] Encode(float[] samples, WaveFormat format)
        {
            var bytesPerSample = format.BitsPerSample / 8;
            var data = new byte[samples.Length * bytesPerSample];
            for (var i = 0; i < samples.Length; i++)
            {
                var sample = Math.Clamp(samples[i], -1f, 1f);
                var offset = i * bytesPerSample;
                switch (format)
                {
                    case { Encoding: WaveFormatEncoding.Pcm, BitsPerSample: 16 }:
                    {
                        var value = (short)Math.Clamp((int)(sample * 32767f), short.MinValue, short.MaxValue);
                        data[offset] = (byte)value;
                        data[offset + 1] = (byte)(value >> 8);
                        break;
                    }
                    case { Encoding: WaveFormatEncoding.Pcm, BitsPerSample: 24 }:
                    {
                        var value = Math.Clamp((int)(sample * 8388607f), -8388608, 8388607);
                        data[offset] = (byte)value;
                        data[offset + 1] = (byte)(value >> 8);
                        data[offset + 2] = (byte)(value >> 16);
                        break;
                    }
                    case { Encoding: WaveFormatEncoding.Pcm, BitsPerSample: 32 }:
                    {
                        var value = (int)Math.Clamp((long)(sample * 2147483647f), int.MinValue, int.MaxValue);
                        BitConverter.TryWriteBytes(data.AsSpan(offset), value);
                        break;
                    }
                    case { Encoding: WaveFormatEncoding.IeeeFloat, BitsPerSample: 32 }:
                        BitConverter.TryWriteBytes(data.AsSpan(offset), sample);
                        break;
                    default:
                        throw new InvalidDataException($"Unsupported target format: {format}.");
                }
            }

            return data;
        }
    }
}
