using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using NAudio.CoreAudioApi;
using NAudio.Wave;

using NUnit.Framework;

using Serilog;
using Serilog.Core;
using Serilog.Events;

using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Audio.Play;

namespace SoundSwitch.Tests;

/// <summary>
/// Regression coverage for the notification-sound playback job (issue #2557, Sentry
/// fingerprints SOUNDSWITCH-4A8/49N/49R/4AC/4A9/4AB/4AH/49B): a vanished or reconfigured
/// audio endpoint is an expected condition for a notification sound. It must be logged at
/// <c>Warning</c> (not <c>Error</c>, which alerts on Sentry), must not throw through
/// <c>OnFailure</c>, and must never hang the job.
/// </summary>
[TestFixture]
public class PlaySoundJobTests
{
    private ILogger? _previousLogger;
    private InMemorySink? _sink;

    [SetUp]
    public void CaptureLogs()
    {
        _previousLogger = Log.Logger;
        _sink = new InMemorySink();
        Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Sink(_sink).CreateLogger();
    }

    [TearDown]
    public void RestoreLogger()
    {
        Log.Logger = _previousLogger;
        _sink = null;
    }

    [Test]
    public void IsExpectedDeviceException_ClassifiesEndpointChurnAsExpected()
    {
        // The CoreAudioApi exception family surfaced by the play job when the endpoint
        // disappears or is reconfigured (issue #2557): all expected, never a genuine defect.
        var exceptions = new Exception[]
        {
            new AudioDeviceDisconnectedException(unchecked((int)0x80070490)),
            new AudioDeviceInUseException(unchecked((int)0x8889000A)),
            new AudioFormatNotSupportedException(unchecked((int)0x88890058)),
            new AudioExclusiveModeNotAllowedException(unchecked((int)0x88890073))
        };

        foreach (var exception in exceptions)
        {
            PlaySoundJob.IsExpectedDeviceException(exception)
                .Should().BeTrue($"because {exception.GetType().Name} is endpoint churn, not a playback defect");
        }
    }

    [Test]
    public void IsExpectedDeviceException_UnexpectedFailuresAreNotExceptionsOfTheDisconnectFamily()
    {
        // Anything outside that family stays a real playback failure logged at Error
        // (deliberate behaviour from issue #2384).
        PlaySoundJob.IsExpectedDeviceException(new InvalidOperationException("no sound cards configured")).Should().BeFalse();
        PlaySoundJob.IsExpectedDeviceException(new OperationCanceledException("job cancelled")).Should().BeFalse();
    }

    [Test]
    public void PlaybackStoppedFailure_DeviceDisconnectIsLoggedAsWarningNotError()
    {
        var job = new PlaySoundJob(null, MakeSilentSound());
        var reportFailure = typeof(PlaySoundJob).GetMethod("ReportPlaybackStoppedFailure", NonPublicInstance);
        reportFailure.Should().NotBeNull("because the format of the playback-stopped failure report must stay inspectable");

        // A disconnect landing on PlaybackStopped must not create a Sentry error event.
        reportFailure!.Invoke(job, new object[] { new AudioDeviceDisconnectedException(unchecked((int)0x80070490)) });

        EventsFor(LogEventLevel.Error).Should().BeEmpty("because a device disconnect during playback is an expected condition (issue #2557)");
        EventsFor(LogEventLevel.Warning).Should().Contain(e => e.RenderMessage().Contains("disconnect or reconfiguration"));
    }

    [Test]
    public async Task ExecuteAsync_MissingConfiguredDevice_CompletesQuicklyWithoutErrorLog()
    {
        // The configured endpoint cannot be found anymore: the job falls back to the default
        // render endpoint (or skips the sound when there is none, e.g. on CI) and must return
        // promptly either way. A hang here is exactly the failure mode of issue #2557.
        var job = new PlaySoundJob("nonexistent-playback-endpoint-" + Guid.NewGuid(), MakeSilentSound());

        await job.ExecuteAsync(CancellationToken.None).WaitAsync(Timeout);

        EventsFor(LogEventLevel.Error).Should().BeEmpty(
            "because a missing endpoint for notification playback is expected, not an error");
    }

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);
    private const BindingFlags NonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;

    /// <summary>
    /// 100 ms of silence, built without touching any audio device (see
    /// <see cref="RefreshDeviceTests"/>: CI has no audio device).
    /// </summary>
    private static CachedSound MakeSilentSound()
    {
        var format = new WaveFormat(8000, 16, 1);
        var bytes = new byte[format.SampleRate / 10 * format.BlockAlign];
        return new CachedSound(new MemoryStream(bytes), format);
    }

    private IReadOnlyList<LogEvent> EventsFor(LogEventLevel level)
    {
        _sink!.Should().NotBeNull();
        lock (_sink)
        {
            return _sink.Events.FindAll(e => e.Level == level);
        }
    }

    private sealed class InMemorySink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = new();

        public void Emit(LogEvent logEvent)
        {
            lock (Events)
            {
                Events.Add(logEvent);
            }
        }
    }
}
