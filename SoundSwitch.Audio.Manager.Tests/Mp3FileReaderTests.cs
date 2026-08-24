using System;
using System.IO;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Playback;

namespace SoundSwitch.Audio.Manager.Tests;

/// <summary>
/// Pins the MP3 decoding contract of the notification sound path: the vendored MP3Sharp decoder
/// (pure managed, no OS dependency) decodes MP3 files to 16-bit stereo PCM matching the source
/// sample rate (mono sources are doubled to stereo), and any input that is not a valid MP3
/// stream is rejected with <see cref="InvalidDataException"/> — mirroring the
/// <see cref="WaveFileReader"/> contract.
/// </summary>
[TestFixture]
public sealed class Mp3FileReaderTests
{
    private static byte[] LoadFixture(string name)
    {
        using var stream = typeof(Mp3FileReaderTests).Assembly
            .GetManifestResourceStream($"SoundSwitch.Audio.Manager.Tests.TestData.{name}");
        if (stream == null) throw new InvalidOperationException($"Missing embedded resource {name}");
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    private static void AssertDecodedMp3(byte[] audioData, WaveFormat format, int lowerBound, int upperBound)
    {
        format.Encoding.Should().Be(WaveFormatEncoding.Pcm);
        format.SampleRate.Should().Be(44100);
        format.Channels.Should().Be(2);
        format.BitsPerSample.Should().Be(16);
        format.BlockAlign.Should().Be(4);
        format.AverageBytesPerSecond.Should().Be(44100 * 4);
        audioData.Should().NotBeEmpty();

        // 5 seconds of 44.1 kHz stereo 16-bit audio (mono fixture doubled to stereo) —
        // allow slack for encoder delay/padding.
        audioData.Length.Should().BeInRange(lowerBound, upperBound);
    }

    [Test]
    public void ReadFile_DecodesRealMp3ToPcm()
    {
        var mp3 = LoadFixture("440Hz-5sec.mp3");
        var path = Path.Combine(Path.GetTempPath(), $"soundswitch-test-{Guid.NewGuid():N}.mp3");
        try
        {
            File.WriteAllBytes(path, mp3);

            var (audioData, format) = Mp3FileReader.ReadFile(path);

            AssertDecodedMp3(audioData, format, 800000, 930000);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void Read_DecodesRealMp3StreamToPcm()
    {
        using var stream = new MemoryStream(LoadFixture("1000Hz-5sec.mp3"));

        var (audioData, format) = Mp3FileReader.Read(stream);

        AssertDecodedMp3(audioData, format, 800000, 930000);
    }

    [Test]
    public void Read_RejectsNonMp3()
    {
        // A PNG header is not an MP3 stream — the reader must reject it.
        byte[] badBytes = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        var act = () => Mp3FileReader.Read(new MemoryStream(badBytes));

        act.Should().Throw<InvalidDataException>();
    }
}
