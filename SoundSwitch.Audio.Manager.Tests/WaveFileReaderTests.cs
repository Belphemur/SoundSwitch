using System;
using System.IO;

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
