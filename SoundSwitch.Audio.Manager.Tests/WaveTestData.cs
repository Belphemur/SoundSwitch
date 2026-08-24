using System;
using System.IO;
using System.Text;

namespace SoundSwitch.Audio.Manager.Tests;

/// <summary>
/// Builds in-memory RIFF/WAVE payloads for the playback tests.
/// </summary>
internal static class WaveTestData
{
    // KSDATAFORMAT_SUBTYPE_PCM / KSDATAFORMAT_SUBTYPE_IEEE_FLOAT (ksmedia.h)
    private static readonly Guid SubTypePcm = new("00000001-0000-0010-8000-00aa00389b71");
    private static readonly Guid SubTypeIeeeFloat = new("00000003-0000-0010-8000-00aa00389b71");

    internal static byte[] BuildWav(ushort formatTag, ushort channels, uint sampleRate, ushort bitsPerSample, byte[] data,
        bool extensible = false, bool includeDataChunk = true, bool oddJunkChunk = false,
        Guid? extensibleSubType = null, bool listChunk = false, bool dataBeforeFmt = false,
        uint? dataChunkSizeOverride = null, byte[]? trailingBytes = null, bool includeFmtChunk = true)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII);
        var blockAlign = (ushort)(channels * (bitsPerSample / 8));

        void WriteFmtChunk()
        {
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(extensible ? (uint)40 : (uint)16);
            writer.Write(extensible ? (ushort)0xFFFE : formatTag);
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * blockAlign);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);
            if (extensible)
            {
                writer.Write((ushort)22); // cbSize
                writer.Write(bitsPerSample); // valid bits per sample
                writer.Write((uint)0); // channel mask
                writer.Write((extensibleSubType ?? (formatTag == 1 ? SubTypePcm : SubTypeIeeeFloat)).ToByteArray());
            }
        }

        void WriteDataChunk()
        {
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dataChunkSizeOverride ?? (uint)data.Length);
            writer.Write(data);
        }

        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write((uint)0); // RIFF size placeholder — the parser must not trust it
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));

        if (oddJunkChunk)
        {
            writer.Write(Encoding.ASCII.GetBytes("JUNK"));
            writer.Write((uint)3);
            writer.Write(new byte[] { 1, 2, 3 });
            writer.Write((byte)0); // word-alignment pad byte
        }

        if (includeDataChunk && dataBeforeFmt)
            WriteDataChunk();

        if (listChunk)
        {
            // Even-sized LIST/INFO metadata chunk — must be skipped without being interpreted.
            writer.Write(Encoding.ASCII.GetBytes("LIST"));
            writer.Write((uint)12);
            writer.Write(Encoding.ASCII.GetBytes("INFOISFTTest"));
        }

        if (includeFmtChunk)
            WriteFmtChunk();

        if (includeDataChunk && !dataBeforeFmt)
            WriteDataChunk();

        if (trailingBytes != null)
            writer.Write(trailingBytes);

        writer.Flush();
        return stream.ToArray();
    }
}
