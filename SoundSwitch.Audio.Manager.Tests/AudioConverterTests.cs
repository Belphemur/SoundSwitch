using System;
using System.IO;

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

    [Test]
    public void Convert_Pcm32ToFloat32_ScalesSamples()
    {
        var pcm = new byte[8];
        BitConverter.GetBytes(int.MaxValue).CopyTo(pcm, 0);
        BitConverter.GetBytes(int.MinValue).CopyTo(pcm, 4);
        var source = new WaveFormat(WaveFormatEncoding.Pcm, 48000, 32, 1);
        var target = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 1);

        var converted = AudioConverter.Convert(pcm, source, target);

        BitConverter.ToSingle(converted, 0).Should().BeApproximately(1f, 1e-6f);
        BitConverter.ToSingle(converted, 4).Should().Be(-1f);
    }

    [Test]
    public void Convert_Float32ToPcm16_EncodesKnownValues()
    {
        // +0.5 → (int)(0.5 * 32767) = 16383 (0x3FFF); -0.5 truncates toward zero → -16383 (0xC001).
        var floats = new byte[8];
        BitConverter.GetBytes(0.5f).CopyTo(floats, 0);
        BitConverter.GetBytes(-0.5f).CopyTo(floats, 4);
        var source = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 1);
        var target = new WaveFormat(WaveFormatEncoding.Pcm, 48000, 16, 1);

        var converted = AudioConverter.Convert(floats, source, target);

        converted.Should().HaveCount(4);
        BitConverter.ToInt16(converted, 0).Should().Be((short)16383);
        BitConverter.ToInt16(converted, 2).Should().Be((short)-16383);
    }

    [Test]
    public void Convert_Float32ToPcm16_RoundTripsWithinOneLsb()
    {
        // Decode scales by 1/32768 but encode scales by 32767 — the round trip is lossy by at
        // most one LSB per sample, which is inaudible for notification sounds.
        var amplitudes = new[] { 0f, 0.25f, 0.5f, -0.5f, 0.999f, -0.999f, 1f, -1f };
        var floats = new byte[amplitudes.Length * 4];
        for (var i = 0; i < amplitudes.Length; i++)
            BitConverter.GetBytes(amplitudes[i]).CopyTo(floats, i * 4);
        var floatFormat = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 1);
        var pcmFormat = new WaveFormat(WaveFormatEncoding.Pcm, 48000, 16, 1);

        var roundTripped = AudioConverter.Convert(AudioConverter.Convert(floats, floatFormat, pcmFormat), pcmFormat, floatFormat);

        for (var i = 0; i < amplitudes.Length; i++)
            BitConverter.ToSingle(roundTripped, i * 4).Should().BeApproximately(amplitudes[i], 1.5f / 32768f);
    }

    [Test]
    public void Convert_Float32ToPcm24_EncodesFullScale()
    {
        var floats = new byte[8];
        BitConverter.GetBytes(1f).CopyTo(floats, 0);
        BitConverter.GetBytes(-1f).CopyTo(floats, 4);
        var source = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 1);
        var target = new WaveFormat(WaveFormatEncoding.Pcm, 48000, 24, 1);

        var converted = AudioConverter.Convert(floats, source, target);

        converted.Should().HaveCount(6);
        // +1 → 8388607 (0x7FFFFF); -1 → -8388607 (0x800001): the [-1,1] clamp is applied before
        // the *8388607 scaling, so the encoder never emits the -8388608 code.
        converted.Should().Equal(0xFF, 0xFF, 0x7F,
            0x01, 0x00, 0x80);
    }

    [Test]
    public void Convert_Float32ToPcm32_EncodesFullScale()
    {
        var floats = new byte[8];
        BitConverter.GetBytes(1f).CopyTo(floats, 0);
        BitConverter.GetBytes(-1f).CopyTo(floats, 4);
        var source = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 1);
        var target = new WaveFormat(WaveFormatEncoding.Pcm, 48000, 32, 1);

        var converted = AudioConverter.Convert(floats, source, target);

        converted.Should().HaveCount(8);
        BitConverter.ToInt32(converted, 0).Should().Be(int.MaxValue);
        BitConverter.ToInt32(converted, 4).Should().Be(int.MinValue);
    }

    [Test]
    public void Convert_AboveFullScale_ClampsToPcmRange()
    {
        var floats = new byte[16];
        BitConverter.GetBytes(2f).CopyTo(floats, 0);
        BitConverter.GetBytes(-2f).CopyTo(floats, 4);
        BitConverter.GetBytes(1.5f).CopyTo(floats, 8);
        BitConverter.GetBytes(-1.5f).CopyTo(floats, 12);
        var source = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 1);
        var target = new WaveFormat(WaveFormatEncoding.Pcm, 48000, 16, 1);

        var converted = AudioConverter.Convert(floats, source, target);

        // Every out-of-range value is clamped to [-1, 1] first, so +over → 32767 and -over → -32767
        // (the encoder never emits the -32768 code).
        BitConverter.ToInt16(converted, 0).Should().Be((short)32767);
        BitConverter.ToInt16(converted, 2).Should().Be((short)-32767);
        BitConverter.ToInt16(converted, 4).Should().Be((short)32767);
        BitConverter.ToInt16(converted, 6).Should().Be((short)-32767);
    }

    [Test]
    public void Convert_NonFiniteSamples_DoNotThrow()
    {
        var floats = new byte[12];
        BitConverter.GetBytes(float.NaN).CopyTo(floats, 0);
        BitConverter.GetBytes(float.PositiveInfinity).CopyTo(floats, 4);
        BitConverter.GetBytes(float.NegativeInfinity).CopyTo(floats, 8);
        var source = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 1);
        var target = new WaveFormat(WaveFormatEncoding.Pcm, 48000, 16, 1);

        var converted = AudioConverter.Convert(floats, source, target);

        converted.Should().HaveCount(6);
        // Infinity clamps to full scale; NaN only has to survive without throwing.
        BitConverter.ToInt16(converted, 2).Should().Be((short)32767);
        BitConverter.ToInt16(converted, 4).Should().Be((short)-32767);
    }

    [Test]
    public void Convert_EmptyInput_ReturnsEmptyOutput()
    {
        var empty = Array.Empty<byte>();
        var pcm16Mono = new WaveFormat(WaveFormatEncoding.Pcm, 48000, 16, 1);
        var float32Mono = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 1);
        var float32Stereo = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 2);
        var float32Mono44k = new WaveFormat(WaveFormatEncoding.IeeeFloat, 44100, 32, 1);

        AudioConverter.Convert(empty, pcm16Mono, float32Mono).Should().BeEmpty();
        AudioConverter.Convert(empty, float32Mono, float32Stereo).Should().BeEmpty(); // channel remap, 0 samples
        AudioConverter.Convert(empty, float32Mono, float32Mono44k).Should().BeEmpty(); // resample, 0 samples
    }

    [Test]
    public void Convert_Resample48kTo44k1_FractionalFrameCount_Truncates()
    {
        // 7 frames at 48 kHz → 6.43 frames at 44.1 kHz: the truncating frame math must yield 6.
        var floats = new byte[7 * 4];
        for (var i = 0; i < 7; i++)
            BitConverter.GetBytes(i / 6f).CopyTo(floats, i * 4);
        var source = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 1);
        var target = new WaveFormat(WaveFormatEncoding.IeeeFloat, 44100, 32, 1);

        var converted = AudioConverter.Convert(floats, source, target);

        converted.Should().HaveCount(6 * 4);
    }

    [Test]
    public void Convert_Resample_IsEndpointPreserving()
    {
        // 10 frames at 48 kHz → 9 frames at 44.1 kHz: first and last output frames land exactly on
        // the first and last input frames (interpolation fraction 0 at both ends).
        var floats = new byte[10 * 4];
        for (var i = 0; i < 10; i++)
            BitConverter.GetBytes(i / 9f).CopyTo(floats, i * 4);
        var source = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 1);
        var target = new WaveFormat(WaveFormatEncoding.IeeeFloat, 44100, 32, 1);

        var converted = AudioConverter.Convert(floats, source, target);

        (converted.Length / 4).Should().Be(9);
        BitConverter.ToSingle(converted, 0).Should().Be(0f);
        BitConverter.ToSingle(converted, 8 * 4).Should().Be(1f);
    }

    [Test]
    public void Convert_ResampleUpsampling_InterpolatesLinearly()
    {
        // 2 frames {0, 1} at 44.1 kHz → 4 frames at 88.2 kHz: {0, 1/3, 2/3, 1}.
        var floats = new byte[8];
        BitConverter.GetBytes(0f).CopyTo(floats, 0);
        BitConverter.GetBytes(1f).CopyTo(floats, 4);
        var source = new WaveFormat(WaveFormatEncoding.IeeeFloat, 44100, 32, 1);
        var target = new WaveFormat(WaveFormatEncoding.IeeeFloat, 88200, 32, 1);

        var converted = AudioConverter.Convert(floats, source, target);

        (converted.Length / 4).Should().Be(4);
        BitConverter.ToSingle(converted, 0).Should().Be(0f);
        BitConverter.ToSingle(converted, 4).Should().BeApproximately(1f / 3, 1e-5f);
        BitConverter.ToSingle(converted, 8).Should().BeApproximately(2f / 3, 1e-5f);
        BitConverter.ToSingle(converted, 12).Should().Be(1f);
    }

    [Test]
    public void Convert_StereoToSixChannels_CopiesSharedAndSilencesRest()
    {
        var floats = new byte[16];
        BitConverter.GetBytes(0.1f).CopyTo(floats, 0);
        BitConverter.GetBytes(0.2f).CopyTo(floats, 4);
        BitConverter.GetBytes(0.3f).CopyTo(floats, 8);
        BitConverter.GetBytes(0.4f).CopyTo(floats, 12);
        var source = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 2);
        var target = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 6);

        var converted = AudioConverter.Convert(floats, source, target);

        converted.Should().HaveCount(12 * 4);
        BitConverter.ToSingle(converted, 0).Should().Be(0.1f);
        BitConverter.ToSingle(converted, 4).Should().Be(0.2f);
        for (var c = 2; c < 6; c++)
            BitConverter.ToSingle(converted, c * 4).Should().Be(0f);
        BitConverter.ToSingle(converted, 6 * 4).Should().Be(0.3f);
        BitConverter.ToSingle(converted, 7 * 4).Should().Be(0.4f);
    }

    [Test]
    public void Convert_UnsupportedSourceFormat_Throws()
    {
        var source = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 64, 1); // 64-bit float
        var target = new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 1);

        var act = () => AudioConverter.Convert(new byte[8], source, target);

        act.Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Convert_UnsupportedTargetFormat_Throws()
    {
        var source = new WaveFormat(WaveFormatEncoding.Pcm, 48000, 16, 1);
        var target = new WaveFormat(WaveFormatEncoding.Pcm, 48000, 8, 1); // 8-bit PCM

        var act = () => AudioConverter.Convert(new byte[2], source, target);

        act.Should().Throw<InvalidDataException>();
    }
}
