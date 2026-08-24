using System;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Playback;

namespace SoundSwitch.Audio.Manager.Tests;

/// <summary>
/// Pins the value contract of the in-house <see cref="WaveFormat"/> record: computed layout
/// properties, record equality, and the KSDATAFORMAT subtype GUID mapping used to decode
/// WAVE_FORMAT_EXTENSIBLE headers.
/// </summary>
[TestFixture]
public sealed class WaveFormatTests
{
    [Test]
    public void BlockAlign_IsBytesPerSampleTimesChannels()
    {
        new WaveFormat(WaveFormatEncoding.Pcm, 44100, 16, 2).BlockAlign.Should().Be(4);
        new WaveFormat(WaveFormatEncoding.Pcm, 48000, 24, 1).BlockAlign.Should().Be(3);
        new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 2).BlockAlign.Should().Be(8);
        new WaveFormat(WaveFormatEncoding.Pcm, 48000, 16, 6).BlockAlign.Should().Be(12); // 5.1
    }

    [Test]
    public void AverageBytesPerSecond_IsSampleRateTimesBlockAlign()
    {
        new WaveFormat(WaveFormatEncoding.Pcm, 44100, 16, 2).AverageBytesPerSecond.Should().Be(176400);
        new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 2).AverageBytesPerSecond.Should().Be(48000 * 8);
        new WaveFormat(WaveFormatEncoding.Pcm, 8000, 16, 1).AverageBytesPerSecond.Should().Be(16000);
    }

    [Test]
    public void IdenticalFormats_AreValueEqual()
    {
        var a = new WaveFormat(WaveFormatEncoding.Pcm, 44100, 16, 2);
        var b = new WaveFormat(WaveFormatEncoding.Pcm, 44100, 16, 2);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Test]
    public void FormatsDifferingInAnyComponent_AreNotEqual()
    {
        var a = new WaveFormat(WaveFormatEncoding.Pcm, 44100, 16, 2);

        a.Should().NotBe(new WaveFormat(WaveFormatEncoding.Pcm, 48000, 16, 2));
        a.Should().NotBe(new WaveFormat(WaveFormatEncoding.IeeeFloat, 44100, 32, 2));
        a.Should().NotBe(new WaveFormat(WaveFormatEncoding.Pcm, 44100, 24, 2));
        a.Should().NotBe(new WaveFormat(WaveFormatEncoding.Pcm, 44100, 16, 1));
    }

    [Test]
    public void ToString_ListsAllFourComponents()
    {
        var format = new WaveFormat(WaveFormatEncoding.Pcm, 44100, 16, 2);

        var text = format.ToString();

        text.Should().Contain("Encoding = Pcm");
        text.Should().Contain("SampleRate = 44100");
        text.Should().Contain("BitsPerSample = 16");
        text.Should().Contain("Channels = 2");
    }

    [Test]
    public void EncodingAndExtensibleTag_MatchNativeWaveFormatTags()
    {
        ((ushort)WaveFormatEncoding.Pcm).Should().Be((ushort)1);
        ((ushort)WaveFormatEncoding.IeeeFloat).Should().Be((ushort)3);
        WaveFormatSubTypes.ExtensibleTag.Should().Be((ushort)0xFFFE);
    }

    [Test]
    public void SubTypeGuids_MapToEncodings()
    {
        WaveFormatSubTypes.TryMap(new Guid("00000001-0000-0010-8000-00aa00389b71"), out var pcm).Should().BeTrue();
        pcm.Should().Be(WaveFormatEncoding.Pcm);

        WaveFormatSubTypes.TryMap(new Guid("00000003-0000-0010-8000-00aa00389b71"), out var ieeeFloat).Should().BeTrue();
        ieeeFloat.Should().Be(WaveFormatEncoding.IeeeFloat);

        WaveFormatSubTypes.TryMap(Guid.Empty, out var unmapped).Should().BeFalse();
        unmapped.Should().Be(default(WaveFormatEncoding));
    }
}
