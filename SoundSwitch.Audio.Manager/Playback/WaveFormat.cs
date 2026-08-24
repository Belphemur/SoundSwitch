#nullable enable
using System;

namespace SoundSwitch.Audio.Manager.Playback
{
    /// <summary>
    /// Wave encoding tags (mmreg.h) supported by the notification playback path.
    /// </summary>
    public enum WaveFormatEncoding : ushort
    {
        Pcm = 1,
        IeeeFloat = 3
    }

    /// <summary>
    /// Format of a PCM/IEEE-float wave buffer — the in-house replacement for the legacy
    /// third-party wave format type. Carries exactly what the WASAPI render path
    /// needs; <see cref="BlockAlign"/> and <see cref="AverageBytesPerSecond"/> are derived.
    /// </summary>
    public sealed record WaveFormat(WaveFormatEncoding Encoding, int SampleRate, int BitsPerSample, int Channels)
    {
        /// <summary>Bytes per sample frame (all channels).</summary>
        public int BlockAlign => Channels * ((BitsPerSample + 7) / 8);

        /// <summary>Bytes per second of audio.</summary>
        public int AverageBytesPerSecond => SampleRate * BlockAlign;
    }

    /// <summary>
    /// WAVE_FORMAT_EXTENSIBLE sub-format GUIDs (ksmedia.h) and their mapping to the base
    /// <see cref="WaveFormatEncoding"/> tags. Shared by the RIFF parser and the WASAPI
    /// mix-format marshaller, both of which can encounter extensible headers.
    /// </summary>
    internal static class WaveFormatSubTypes
    {
        // KSDATAFORMAT_SUBTYPE_PCM
        internal static readonly Guid Pcm = new("00000001-0000-0010-8000-00aa00389b71");
        // KSDATAFORMAT_SUBTYPE_IEEE_FLOAT
        internal static readonly Guid IeeeFloat = new("00000003-0000-0010-8000-00aa00389b71");

        // WAVE_FORMAT_EXTENSIBLE (mmreg.h)
        internal const ushort ExtensibleTag = 0xFFFE;

        internal static bool TryMap(Guid subType, out WaveFormatEncoding encoding)
        {
            if (subType == Pcm)
            {
                encoding = WaveFormatEncoding.Pcm;
                return true;
            }

            if (subType == IeeeFloat)
            {
                encoding = WaveFormatEncoding.IeeeFloat;
                return true;
            }

            encoding = default;
            return false;
        }
    }
}
