using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MP3Sharp;

using NUnit.Framework;

namespace SoundSwitch.Mp3Sharp.Tests;

/// <summary>
/// Pins the decoding contract of the pure-managed <see cref="MP3Stream"/> decoder: real MP3
/// fixtures decode to 16-bit stereo PCM at 44.1 kHz, decodes are deterministic across buffer
/// sizes, instances, and threads, and non-MP3 or empty input is rejected from the constructor
/// with <see cref="MP3SharpException"/>. All assertions are Windows-independent and work purely
/// off the public <see cref="MP3Stream"/> API.
/// </summary>
[TestFixture]
public sealed class MP3StreamTests
{
    private static byte[] LoadFixture(string name)
    {
        using var stream = typeof(MP3StreamTests).Assembly
            .GetManifestResourceStream($"SoundSwitch.Mp3Sharp.Tests.TestData.{name}");
        if (stream == null) throw new InvalidOperationException($"Missing embedded resource {name}");
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    private static byte[] ReadAllPcm(MP3Stream mp3)
    {
        var buffer = new byte[8192];
        using var pcm = new MemoryStream();
        while (true)
        {
            var read = mp3.Read(buffer, 0, buffer.Length);
            if (read == 0)
                break;
            pcm.Write(buffer, 0, read);
        }
        return pcm.ToArray();
    }

    private static byte[] DecodeAll(byte[] mp3Bytes)
    {
        using var mp3 = new MP3Stream(new MemoryStream(mp3Bytes));
        return ReadAllPcm(mp3);
    }

    [Test]
    public void Decode_Fixture_ProducesPcm()
    {
        var mp3Bytes = LoadFixture("440Hz-5sec.mp3");

        using var mp3 = new MP3Stream(new MemoryStream(mp3Bytes));
        mp3.Frequency.Should().Be(44100);
        var pcm = ReadAllPcm(mp3);

        // 5 seconds of 44.1 kHz audio — allow slack for encoder delay/padding.
        pcm.Length.Should().BeInRange(790000, 930000);

        // Real audio, not silence: at least one 1 KB block past the (small) lead-in must be non-zero.
        var hasAudio = false;
        for (var offset = 4096; offset < pcm.Length; offset += 1024)
        {
            if (pcm.Skip(offset).Take(1024).Any(b => b != 0))
            {
                hasAudio = true;
                break;
            }
        }
        hasAudio.Should().BeTrue();
    }

    [Test]
    public void Decode_ProducesStereo16Bit()
    {
        var pcm = DecodeAll(LoadFixture("440Hz-5sec.mp3"));

        // 16-bit stereo implies even byte counts and 4 bytes/frame at 44.1 kHz.
        (pcm.Length % 2).Should().Be(0);
        pcm.Length.Should().BeInRange(790000, 930000);
    }

    [Test]
    public void Read_SmallBuffers_MatchesFullRead()
    {
        var mp3Bytes = LoadFixture("440Hz-5sec.mp3");

        var fullRead = DecodeAll(mp3Bytes);

        using var mp3 = new MP3Stream(new MemoryStream(mp3Bytes));
        // The vendored decoder's output buffer only serves whole 16-bit stereo sample frames,
        // so the minimum buffer that does not deadlock is 4 bytes (a single frame).
        var smallBuffer = new byte[4];
        using var pcm = new MemoryStream();
        while (true)
        {
            var read = mp3.Read(smallBuffer, 0, smallBuffer.Length);
            if (read == 0)
                break;
            pcm.Write(smallBuffer, 0, read);
        }

        pcm.ToArray().Should().Equal(fullRead);
    }

    [Test]
    public void SecondDecode_IsIdentical()
    {
        var mp3Bytes = LoadFixture("440Hz-5sec.mp3");

        var first = DecodeAll(mp3Bytes);
        var second = DecodeAll(mp3Bytes);

        second.Should().Equal(first);
    }

    [Test]
    public void Decode_NonMp3_Throws()
    {
        // A PNG header is not an MP3 stream — the constructor must reject it.
        byte[] pngHeaderBytes = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        var act = () => new MP3Stream(new MemoryStream(pngHeaderBytes));

        act.Should().Throw<MP3SharpException>();
    }

    [Test]
    public void Decode_EmptyStream_Throws()
    {
        var act = () => new MP3Stream(new MemoryStream(Array.Empty<byte>()));

        act.Should().Throw<MP3SharpException>();
    }

    [Test]
    public async Task Parallel_Decodes_AreStable()
    {
        var mp3Bytes = LoadFixture("1000Hz-5sec.mp3");

        var results = await Task.WhenAll(
            Enumerable.Range(0, 8).Select(_ => Task.Run(() => DecodeAll(mp3Bytes))));

        results.Should().OnlyContain(result => result.SequenceEqual(results[0]));
    }
}