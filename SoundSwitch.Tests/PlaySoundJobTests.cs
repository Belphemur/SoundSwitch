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
            new AudioExclusiveModeNotAllowedException(unchecked((int)0x88890073)),
            // A plain E_NOTFOUND wraps the "no default endpoint exists" answer (the last render
            // endpoint vanished between resolution and player creation — WasapiPlayerBuilder
            // fallback)
            new CoreAudioException(unchecked((int)0x80070490))
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

    [Test]
    public async Task PlayToCompletionAsync_LostCompletionSignal_IsAbandonedWithoutBlockingTheJob()
    {
        // Deterministic stand-in for the failure mode of issue #2557: the endpoint died
        // mid-playback and PlaybackStopped was never raised. The bounded wait must end the
        // job (return once, at Warning), and the teardown must be handed to the background:
        // WasapiPlayer joins its render thread synchronously on Stop()/Dispose(), and that
        // thread can be wedged inside a driver call, so releasing it here would turn the
        // bounded wait into an unbounded join again.
        var job = new PlaySoundJob(null, MakeSilentSound()) { PlaybackCompletionTimeout = TimeSpan.FromMilliseconds(150) };
        var player = new PlayerNeverCompleting();

        await job.PlayToCompletionAsync(player, CancellationToken.None).WaitAsync(Timeout);

        player.Played.Should().BeTrue("because the abandon path runs after playback started");
        (await player.DisposalStarted.Task.WaitAsync(Timeout)).Should()
            .BeTrue("because the teardown of the abandoned playback must be detached from the job, not awaited by it");

        EventsFor(LogEventLevel.Error).Should().BeEmpty("because losing the completion signal is expected after a disconnect (issue #2557)");
        EventsFor(LogEventLevel.Warning).Should().Contain(e => e.RenderMessage().Contains("abandoning it"));
    }

    [Test]
    public async Task PlayToCompletionAsync_CancelledWhilePlaying_TeardownIsDetachedNotBlocking()
    {
        // NotificationSound cancels the previous job before scheduling the next one: the wait
        // throws OperationCanceledException, and the player must then be torn down in the
        // background (same wedged-render-thread risk as the lost-signal case), never by a
        // synchronous join on the job. The cancellation still propagates.
        var job = new PlaySoundJob(null, MakeSilentSound());
        var player = new PlayerNeverCompleting();

        Func<Task> act = async () =>
        {
            using var cancellationSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            await job.PlayToCompletionAsync(player, cancellationSource.Token).WaitAsync(Timeout);
        };

        await act.Should().ThrowAsync<OperationCanceledException>();

        (await player.DisposalStarted.Task.WaitAsync(Timeout)).Should()
            .BeTrue("because the cancellation path detaches the teardown instead of joining the render thread synchronously");

        EventsFor(LogEventLevel.Error).Should().BeEmpty("because a cancelled playback is not a failure");
    }

    /// <summary>
    /// A player double that starts but never raises <c>PlaybackStopped</c>, and whose
    /// <c>DisposeAsync</c> records that teardown was (detached) started but never returns
    /// until released — mimicking NAudio parking the render-thread join.
    /// </summary>
    private sealed class PlayerNeverCompleting : IWavePlayer, IAsyncDisposable
    {
        private readonly TaskCompletionSource<bool> _disposalStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _disposalReleased = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool Played { get; private set; }
        public TaskCompletionSource<bool> DisposalStarted => _disposalStarted;

        public event EventHandler<StoppedEventArgs>? PlaybackStopped;

        public PlaybackState PlaybackState { get; private set; }
        public WaveFormat OutputWaveFormat { get; } = new WaveFormat(8000, 16, 1);
        public float Volume { get; set; }

        public void Init(IWaveProvider waveStream)
        {
            PlaybackState = PlaybackState.Stopped;
        }

        public void Play()
        {
            Played = true;
            PlaybackState = PlaybackState.Playing;
        }

        public void Pause()
        {
        }

        public void Stop()
        {
            // Not called on the abandon path any more: teardown is detached.
            PlaybackState = PlaybackState.Stopped;
        }

        public ValueTask DisposeAsync()
        {
            _disposalStarted.TrySetResult(true);
            return new ValueTask(_disposalReleased.Task);
        }

        public void Dispose()
        {
            PlaybackState = PlaybackState.Stopped;
        }
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
        // Same monitor as InMemorySink.Emit, or this enumeration races the writes coming from
        // the playback thread while the job runs.
        lock (_sink.Events)
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
