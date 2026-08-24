using System;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Playback;

namespace SoundSwitch.Audio.Manager.Tests;

/// <summary>
/// Pins the sample conversion contract used when the WASAPI engine's mix format differs from the
/// notification WAV (the common case: 44.1 kHz / 16-bit PCM source, 48 kHz / 32-bit float engine).
/// </summary>
[TestFixture]
public sealed class AudioConverterTests
{
    [Test]
    public void Convert_SameFormat_ReturnsSourceUnchanged()
    {
        var format = new WaveFormat(WaveFormatEncoding.Pcm, 44100, 16, 2);
        var data = new byte[] { 1, 2, 3, 4 };

        AudioConverter.Convert(data, format, format).Should().BeSameAs(data);
    }

    [Test]
    public void Convert_Pcm16ToFloat32_ScalesSamples()
    {
        var pcm = new byte[] { 0xFF, 0x7F, 0x00, 0x80 }; // max positive, max negative
        var source = new WaveFormat(WaveFormatEncoding.Pcm, 48000, 16, 1);
        var target = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 1);

        var converted = AudioConverter.Convert(pcm, source, target);

        converted.Should().HaveCount(8);
        BitConverter.ToSingle(converted, 0).Should().BeApproximately(1f, 0.0001f);
        BitConverter.ToSingle(converted, 4).Should().Be(-1f);
    }

    [Test]
    public void Convert_Pcm24ToFloat32_SignExtends()
    {
        var pcm = new byte[] { 0xFF, 0xFF, 0x7F, 0x00, 0x00, 0x80 }; // max positive, max negative
        var source = new WaveFormat(WaveFormatEncoding.Pcm, 48000, 24, 1);
        var target = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 1);

        var converted = AudioConverter.Convert(pcm, source, target);

        BitConverter.ToSingle(converted, 0).Should().BeApproximately(1f, 0.0001f);
        BitConverter.ToSingle(converted, 4).Should().Be(-1f);
    }

    [Test]
    public void Convert_Resample44kTo48k_ProducesTargetFrameCount()
    {
        var pcm = new byte[441 * 2]; // 441 mono frames at 44.1 kHz = 10 ms
        var source = new WaveFormat(WaveFormatEncoding.Pcm, 44100, 16, 1);
        var target = new WaveFormat(WaveFormatEncoding.Pcm, 48000, 16, 1);

        var converted = AudioConverter.Convert(pcm, source, target);

        (converted.Length / target.BlockAlign).Should().Be(480);
    }

    [Test]
    public void Convert_MonoToStereo_DuplicatesChannel()
    {
        var pcm = new byte[] { 0x00, 0x40 }; // 0x4000 = exactly 0.5 once normalized
        var source = new WaveFormat(WaveFormatEncoding.Pcm, 48000, 16, 1);
        var target = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 2);

        var converted = AudioConverter.Convert(pcm, source, target);

        converted.Should().HaveCount(8);
        BitConverter.ToSingle(converted, 0).Should().Be(0.5f);
        BitConverter.ToSingle(converted, 4).Should().Be(0.5f);
    }

    [Test]
    public void Convert_StereoToMono_AveragesChannels()
    {
        var pcm = new byte[] { 0x00, 0x40, 0x00, 0xC0 }; // +0.5, -0.5 → 0
        var source = new WaveFormat(WaveFormatEncoding.Pcm, 48000, 16, 2);
        var target = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 1);

        var converted = AudioConverter.Convert(pcm, source, target);

        converted.Should().HaveCount(4);
        BitConverter.ToSingle(converted, 0).Should().Be(0f);
    }
}
