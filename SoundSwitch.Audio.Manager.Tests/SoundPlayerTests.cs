using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Playback;

namespace SoundSwitch.Audio.Manager.Tests;

/// <summary>
/// Facade lifecycle contract: cancellation stops promptly without invoking the completion
/// callback, and playback of a tiny buffer settles quickly whether or not the machine has a
/// render endpoint (CI has none — the device-missing path completes with a warning).
/// </summary>
[TestFixture]
public sealed class SoundPlayerTests
{
    private static (byte[] Data, WaveFormat Format) Silence()
    {
        // 10 ms of 8 kHz / 16-bit mono silence — inaudible on machines that do have an endpoint.
        var wav = WaveTestData.BuildWav(formatTag: 1, channels: 1, sampleRate: 8000, bitsPerSample: 16, data: new byte[160]);
        return WaveFileReader.Read(new MemoryStream(wav));
    }

    [Test]
    public async Task PlayAsync_PreCancelled_CompletesWithoutInvokingCallback()
    {
        var (data, format) = Silence();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var callbackInvoked = false;

        await SoundPlayer.PlayAsync(data, format, cancellationToken: cancellation.Token, onCompleted: _ => callbackInvoked = true);

        callbackInvoked.Should().BeFalse();
    }

    [Test]
    public async Task PlayAsync_Silence_CompletesWithoutFaulting()
    {
        var (data, format) = Silence();

        var playTask = SoundPlayer.PlayAsync(data, format);
        var settled = await Task.WhenAny(playTask, Task.Delay(TimeSpan.FromSeconds(15)));

        settled.Should().Be(playTask, "playback of a 10 ms buffer must settle quickly");
        await playTask; // must not fault
    }

    [Test]
    public void PlayAsync_NullData_Throws()
    {
        var act = () => SoundPlayer.PlayAsync(null!, new WaveFormat(WaveFormatEncoding.Pcm, 8000, 16, 1));

        act.Should().Throw<ArgumentNullException>();
    }
}
