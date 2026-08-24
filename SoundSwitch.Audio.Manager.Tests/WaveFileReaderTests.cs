using System;
using System.IO;
using System.Text;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Playback;

namespace SoundSwitch.Audio.Manager.Tests;

/// <summary>
/// Pins the RIFF/WAVE parsing contract of the notification sound path: PCM (16/24/32-bit) and
/// IEEE float (32-bit) WAV only, including WAVE_FORMAT_EXTENSIBLE headers; everything else is
/// rejected with <see cref="InvalidDataException"/>.
/// </summary>
[TestFixture]
public sealed class WaveFileReaderTests
{
    [Test]
    public void Read_Pcm16_ParsesFormatAndData()
    {
        var samples = new byte[] { 0x00, 0x00, 0xFF, 0x7F, 0x00, 0x80, 0x34, 0x12 };
        var wav = WaveTestData.BuildWav(formatTag: 1, channels: 2, sampleRate: 44100, bitsPerSample: 16, data: samples);

        var (audioData, format) = WaveFileReader.Read(new MemoryStream(wav));

        format.Encoding.Should().Be(WaveFormatEncoding.Pcm);
        format.SampleRate.Should().Be(44100);
        format.Channels.Should().Be(2);
        format.BitsPerSample.Should().Be(16);
        format.BlockAlign.Should().Be(4);
        format.AverageBytesPerSecond.Should().Be(44100 * 4);
        audioData.Should().Equal(samples);
    }

    [Test]
    public void Read_IeeeFloat32_ParsesFormat()
    {
        var wav = WaveTestData.BuildWav(formatTag: 3, channels: 1, sampleRate: 48000, bitsPerSample: 32, data: new byte[400]);

        var (_, format) = WaveFileReader.Read(new MemoryStream(wav));

        format.Encoding.Should().Be(WaveFormatEncoding.IeeeFloat);
        format.SampleRate.Should().Be(48000);
        format.BlockAlign.Should().Be(4);
    }

    [Test]
    public void Read_ExtensiblePcm_MapsToPcmEncoding()
    {
        var wav = WaveTestData.BuildWav(formatTag: 1, channels: 2, sampleRate: 48000, bitsPerSample: 32, data: new byte[800], extensible: true);

        var (_, format) = WaveFileReader.Read(new MemoryStream(wav));

        format.Encoding.Should().Be(WaveFormatEncoding.Pcm);
        format.SampleRate.Should().Be(48000);
        format.BitsPerSample.Should().Be(32);
    }

    [Test]
    public void Read_NotRiff_Throws()
    {
        var act = () => WaveFileReader.Read(new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }));

        act.Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_UnsupportedEncoding_Throws()
    {
        // 0x0055 = MP3-in-WAV — custom notification sounds are WAV PCM/float only.
        var wav = WaveTestData.BuildWav(formatTag: 0x0055, channels: 2, sampleRate: 44100, bitsPerSample: 16, data: new byte[100]);

        var act = () => WaveFileReader.Read(new MemoryStream(wav));

        act.Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_Pcm8Bit_Throws()
    {
        var wav = WaveTestData.BuildWav(formatTag: 1, channels: 1, sampleRate: 8000, bitsPerSample: 8, data: new byte[80]);

        var act = () => WaveFileReader.Read(new MemoryStream(wav));

        act.Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_MissingDataChunk_Throws()
    {
        var wav = WaveTestData.BuildWav(formatTag: 1, channels: 2, sampleRate: 44100, bitsPerSample: 16, data: Array.Empty<byte>(), includeDataChunk: false);

        var act = () => WaveFileReader.Read(new MemoryStream(wav));

        act.Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_OddSizedUnknownChunk_IsSkippedWithPadding()
    {
        var samples = new byte[] { 0x2A, 0x00 };
        var wav = WaveTestData.BuildWav(formatTag: 1, channels: 1, sampleRate: 8000, bitsPerSample: 16, data: samples, oddJunkChunk: true);

        var (audioData, _) = WaveFileReader.Read(new MemoryStream(wav));

        audioData.Should().Equal(samples);
    }

    [Test]
    public void Read_DataChunkBeforeFmtChunk_ParsesBoth()
    {
        // Chunks may appear in any order — fmt-before-data is conventional, not required.
        var samples = new byte[] { 0x11, 0x22, 0x33, 0x44 };
        var wav = WaveTestData.BuildWav(formatTag: 1, channels: 1, sampleRate: 8000, bitsPerSample: 16, data: samples, dataBeforeFmt: true);

        var (audioData, format) = WaveFileReader.Read(new MemoryStream(wav));

        format.SampleRate.Should().Be(8000);
        audioData.Should().Equal(samples);
    }

    [Test]
    public void Read_ListInfoChunk_IsSkipped()
    {
        var samples = new byte[] { 0x55, 0x66 };
        var wav = WaveTestData.BuildWav(formatTag: 1, channels: 1, sampleRate: 8000, bitsPerSample: 16, data: samples, listChunk: true);

        var (audioData, _) = WaveFileReader.Read(new MemoryStream(wav));

        audioData.Should().Equal(samples);
    }

    [Test]
    public void Read_Pcm24_DataIsByteExact()
    {
        // One stereo 24-bit frame: left = 0x7FFFFF (max), right = 0x800000 (min).
        var samples = new byte[] { 0xFF, 0xFF, 0x7F, 0x00, 0x00, 0x80 };
        var wav = WaveTestData.BuildWav(formatTag: 1, channels: 2, sampleRate: 48000, bitsPerSample: 24, data: samples);

        var (audioData, format) = WaveFileReader.Read(new MemoryStream(wav));

        format.BitsPerSample.Should().Be(24);
        format.BlockAlign.Should().Be(6);
        audioData.Should().Equal(samples);
    }

    [Test]
    public void Read_TrailingBytesAfterDataChunk_AreIgnored()
    {
        var samples = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var wav = WaveTestData.BuildWav(formatTag: 1, channels: 1, sampleRate: 8000, bitsPerSample: 16, data: samples,
            trailingBytes: new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xCA, 0xFE, 0xBA, 0xBE, 0x00 });

        var (audioData, _) = WaveFileReader.Read(new MemoryStream(wav));

        audioData.Should().Equal(samples);
    }

    [Test]
    public void Read_TruncatedDataChunk_ClampsToAvailableBytes()
    {
        // The chunk header claims 64 bytes but only 4 were written — clamp to what the stream holds.
        var samples = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD };
        var wav = WaveTestData.BuildWav(formatTag: 1, channels: 1, sampleRate: 8000, bitsPerSample: 16, data: samples, dataChunkSizeOverride: 64);

        var (audioData, _) = WaveFileReader.Read(new MemoryStream(wav));

        audioData.Should().Equal(samples);
    }

    [Test]
    public void Read_DeclaredDataSizeSmallerThanPayload_TruncatesToDeclaredSize()
    {
        // 8 bytes of payload but the chunk declares 2 — the declared chunk boundary wins.
        var samples = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var wav = WaveTestData.BuildWav(formatTag: 1, channels: 1, sampleRate: 8000, bitsPerSample: 16, data: samples, dataChunkSizeOverride: 2);

        var (audioData, _) = WaveFileReader.Read(new MemoryStream(wav));

        audioData.Should().Equal(new byte[] { 1, 2 });
    }

    [Test]
    public void Read_EmptyDataChunk_ParsesWithEmptyAudioData()
    {
        var wav = WaveTestData.BuildWav(formatTag: 1, channels: 1, sampleRate: 8000, bitsPerSample: 16, data: Array.Empty<byte>());

        var (audioData, format) = WaveFileReader.Read(new MemoryStream(wav));

        format.SampleRate.Should().Be(8000);
        audioData.Should().BeEmpty();
    }

    [Test]
    public void Read_MissingFmtChunk_Throws()
    {
        var wav = WaveTestData.BuildWav(formatTag: 1, channels: 1, sampleRate: 8000, bitsPerSample: 16, data: new byte[4], includeFmtChunk: false);

        var act = () => WaveFileReader.Read(new MemoryStream(wav));

        act.Should().Throw<InvalidDataException>().WithMessage("*no fmt chunk*");
    }

    [Test]
    public void Read_ExtensibleFloat_MapsToIeeeFloatEncoding()
    {
        var wav = WaveTestData.BuildWav(formatTag: 3, channels: 2, sampleRate: 48000, bitsPerSample: 32, data: new byte[400], extensible: true);

        var (_, format) = WaveFileReader.Read(new MemoryStream(wav));

        format.Encoding.Should().Be(WaveFormatEncoding.IeeeFloat);
        format.BitsPerSample.Should().Be(32);
    }

    [Test]
    public void Read_ExtensibleUnknownSubType_Throws()
    {
        var wav = WaveTestData.BuildWav(formatTag: 1, channels: 2, sampleRate: 48000, bitsPerSample: 16, data: new byte[64], extensible: true,
            extensibleSubType: new Guid("12345678-1234-1234-1234-123456789ABC"));

        var act = () => WaveFileReader.Read(new MemoryStream(wav));

        act.Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_ExtensibleTooSmallCbsize_Throws()
    {
        // WAVE_FORMAT_EXTENSIBLE requires cbSize >= 22 — build a truncated extensible header by hand.
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII);
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write((uint)0);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write((uint)18);
        writer.Write((ushort)0xFFFE);
        writer.Write((ushort)2);
        writer.Write((uint)48000);
        writer.Write((uint)48000 * 8);
        writer.Write((ushort)8);
        writer.Write((ushort)32);
        writer.Write((ushort)0); // cbSize = 0 — too small for the extensible tail
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write((uint)4);
        writer.Write(new byte[4]);
        writer.Flush();

        var act = () => WaveFileReader.Read(new MemoryStream(stream.ToArray()));

        act.Should().Throw<InvalidDataException>().WithMessage("*EXTENSIBLE*");
    }

    [Test]
    public void ReadFile_ParsesFromDisk()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, WaveTestData.BuildWav(formatTag: 1, channels: 1, sampleRate: 8000, bitsPerSample: 16, data: new byte[160]));

            var (audioData, format) = WaveFileReader.ReadFile(path);

            format.SampleRate.Should().Be(8000);
            audioData.Should().HaveCount(160);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
