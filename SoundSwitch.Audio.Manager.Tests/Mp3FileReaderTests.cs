using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Playback;

namespace SoundSwitch.Audio.Manager.Tests;

/// <summary>
/// Pins the MP3 decoding contract of the notification sound path: Media Foundation decodes MP3
/// files to 16-bit PCM matching the source sample rate and channel count, and any input that is
/// not a valid MP3 stream is rejected with <see cref="InvalidDataException"/> — mirroring the
/// <see cref="WaveFileReader"/> contract. Decoding requires Media Foundation, so the tests are
/// skipped on non-Windows platforms.
/// </summary>
[TestFixture]
public sealed class Mp3FileReaderTests
{
    /// <summary>
    /// 200 ms of a 440 Hz sine, encoded as 32 kbps mono 44.1 kHz MP3 (ID3v2 header + LAME frames).
    /// Generated with: ffmpeg -f lavfi -i sine=frequency=440:duration=0.2 -codec:a libmp3lame -b:a 32k -ar 44100 -ac 1
    /// </summary>
    private const string SineMp3Base64 =
        "SUQzBAAAAAAAI1RTU0UAAAAPAAADTGF2ZjYyLjEyLjEwMgAAAAAAAAAAAAAA//tAwAAAAAAAAAAAAAAAAAAAAAAASW5mbwAAAA8AAAAJAAAEYgBBQUFBQUFBQUFBQVlZWVlZWVlZWVlZcHBwcHBwcHBwcHCIiIiIiIiIiIiIiKCgoKCgoKCgoKCguLi4uLi4uLi4uLjQ0NDQ0NDQ0NDQ0Ojo6Ojo6Ojo6Ojo//////////////8AAAAATGF2YzYyLjI4AAAAAAAAAAAAAAAAJAPMAAAAAAAABGIq/cMGAAAAAAD/+xDEAAAEhBVZVGCAMKoIqIM2UAAAAaBLgGACZNPYBAAAELE4Pl3uBA5+oEAQdLg+H8QAhEjv//QDQoE2kAGAxEgMJIjTg9wgV0XQ8koWDOVR/l4CgXwFEg9+HgVO9QNCX51SLmlzTP/7EsQCA8UQHSId4AAoowQiwa9oSAcAlEQDhgAgZGbu2CZWgx5hiA6mCkBmYDIExgQgPGBKAsXi6lUDMUONWxPF8MQEa02/taTbVGvMQQIw99c2Lk1TA1eMyY9gkvwBz8966jDQ4w4dMf/7EMQDA8UEHxgN+yJArYRigb9sSNODPoMwrxvjUM4cNOsbQwngbTXMAgZuqHV+awbJpaHP1/QkQYmImbHBu8aYlw7RwV9cG/wO4YmoT5wjIZ4jGZn5mTwYsLMHjFPgH/q+ijChEw8c//sSxAKDxQAfGA37IkCrBCKBv2xIMaOzPYYwphyTTb5rNLgb8wlwcDSRAQRvJnb4a4TNZ4N/r+hH8xQOM5NTeoQxNRxzhW4hOCMckxOgmjhWUztHMwPjL30xUXXRL6gOfs+6MLEDDB8x//sQxAMDxQAfGA37IkCnBCKBr2xIk8M4iTCfHSNKTrQ0hRyDCMB1M1QDCnCgd3JsAs2nQ5+n6EkDLjTdMD+/zE6GwOH/YI4ShtDE/CVOGYjOkgy9BMsfjEhhdcozBP93qjChEw0eMWP/+xLEA4PE/B8YDfsiQJyD4wGvaEzTNYwwlB3TRv77NE4dMwhQeDIZBxRxGnjoAsl+zwb/T9ivzFADTnTuVDDxE3NqSeU2exOzDyBmPDFNKxM0sMzdMONa5Tgmc+swoowxkx7Q03sweRr/+xDEBgPEpB8aDXsiYJAD44GvaE1jOu1aM5UZwwaAawCoHCm6Mc04KlZtOmv1NABwczAw4Lcwtg3DUtc0NQwN4wuQRTiLDKHjGnTFQgIEf+oGVTCCzCmDGNTR/DBuGtM3vZEzYBpTBv/7EsQKg8SsHxoNeyJgloQkgroQBRBsBi4sUbgp0xA6poM8G/1QGhPAQPmDgoGMBpH3nknrq5mXY/mK4NmA4UmEoOmEIRoT37t6AAQCKxYLRaLRaAAAAAAE4D4il+/CzZC5f8DQOBPK7//7EMQPAAgEfWG49IAYAAA0g4AABMJhQhFP5ICYJgBWl/ycMIIekSL//YQRtxQq1///NgznAqKqTEFNRTMuMTAxIChiZXRhIDMpqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqq";

    private static byte[] SineMp3 => Convert.FromBase64String(SineMp3Base64);

    private static void RequireWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.Ignore("MP3 decoding requires Windows Media Foundation");
        }
    }

    private static void AssertDecodedSine(byte[] audioData, WaveFormat format)
    {
        format.Encoding.Should().Be(WaveFormatEncoding.Pcm);
        format.SampleRate.Should().Be(44100);
        format.Channels.Should().Be(1);
        format.BitsPerSample.Should().Be(16);
        format.BlockAlign.Should().Be(2);
        format.AverageBytesPerSecond.Should().Be(44100 * 2);
        audioData.Should().NotBeEmpty();

        // 200 ms of 44.1 kHz mono 16-bit audio — allow slack for encoder delay/padding.
        audioData.Length.Should().BeInRange(44100 * 2 * 150 / 1000, 44100 * 2 * 300 / 1000);
    }

    [Test]
    public void ReadFile_DecodesMp3ToPcm()
    {
        RequireWindows();
        var path = Path.Combine(Path.GetTempPath(), $"soundswitch-test-{Guid.NewGuid():N}.mp3");
        try
        {
            File.WriteAllBytes(path, SineMp3);

            var (audioData, format) = Mp3FileReader.ReadFile(path);

            AssertDecodedSine(audioData, format);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void Read_DecodesMp3StreamToPcm()
    {
        RequireWindows();
        using var stream = new MemoryStream(SineMp3);

        var (audioData, format) = Mp3FileReader.Read(stream);

        AssertDecodedSine(audioData, format);
    }

    [Test]
    public void Read_GarbageBytes_Throws()
    {
        RequireWindows();
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes("this is definitely not an mp3 file at all"));

        var act = () => Mp3FileReader.Read(stream);

        act.Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_WavBytes_Throws()
    {
        RequireWindows();
        // A well-formed WAV is not an MP3 — the MP3 reader must reject it, the caller
        // (CachedSound) is the one responsible for dispatching WAV vs MP3.
        var wav = WaveTestData.BuildWav(formatTag: 1, channels: 1, sampleRate: 8000, bitsPerSample: 16, data: new byte[160]);

        var act = () => Mp3FileReader.Read(new MemoryStream(wav));

        act.Should().Throw<InvalidDataException>();
    }

    [Test]
    public void ReadFile_MissingFile_Throws()
    {
        RequireWindows();
        var path = Path.Combine(Path.GetTempPath(), $"soundswitch-missing-{Guid.NewGuid():N}.mp3");

        var act = () => Mp3FileReader.ReadFile(path);

        act.Should().Throw<FileNotFoundException>();
    }
}
