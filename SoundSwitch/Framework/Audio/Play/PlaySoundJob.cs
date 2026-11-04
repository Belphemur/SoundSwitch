using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Job.Scheduler.Job;
using Job.Scheduler.Job.Action;
using Job.Scheduler.Job.Exception;

using NAudio.CoreAudioApi;
using NAudio.Wave;

using Serilog;
using Serilog.Events;

namespace SoundSwitch.Framework.Audio.Play;

public class PlaySoundJob([CanBeNull] string deviceId, [NotNull] CachedSound sound) : IJob
{
    /// <summary>
    /// The id for the logs summary: the raw id if given, a placeholder otherwise (empty id means default device).
    /// </summary>
    private string DisplayDeviceId => string.IsNullOrEmpty(deviceId) ? "<default>" : deviceId;

    /// <summary>
    /// How long to wait for the play thread to report completion before giving up on it. Matches
    /// the job scheduler <see cref="MaxRuntime"/> so that a lost <c>PlaybackStopped</c> signal
    /// (possible when the endpoint dies mid-playback) cannot leave the job waiting forever.
    /// </summary>
    private static readonly TimeSpan PlaybackCompletionTimeout = TimeSpan.FromSeconds(30);

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (sound == null)
            throw new ArgumentNullException(nameof(sound));

        try
        {
            await PlaySoundInternalAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
            // Let OnFailure handle all failure logging
            throw;
        }
    }

    private async Task PlaySoundInternalAsync(CancellationToken cancellationToken)
    {
        using var semaphore = new SemaphoreSlim(0);

        using var enumerator = new MMDeviceEnumerator();

        try
        {
            using var device = GetDevice(enumerator);
            if (device == null)
            {
                Log.ForContext<PlaySoundJob>().Warning("No audio device found for specified ID: {DeviceId}.", DisplayDeviceId);
                return;
            }

            using var player = CreatePlayer(device);
            await using var waveStream = new CachedSoundWaveStream(sound);

            player.Init(waveStream);

            void OnPlaybackStoppedHandler(object o, StoppedEventArgs stoppedEventArgs)
            {
                if (stoppedEventArgs.Exception != null)
                {
                    ReportPlaybackStoppedFailure(stoppedEventArgs.Exception);
                }

                try
                {
                    semaphore.Release();
                }
                catch (ObjectDisposedException)
                {
                }
            }

            player.PlaybackStopped += OnPlaybackStoppedHandler;
            try
            {
                player.Play();
                // The play thread raises PlaybackStopped even on a device failure, but if that
                // signal is ever lost (endpoint killed mid-playback), this bounded wait abandons
                // the playback instead of hanging the job until its scheduler runtime.
                if (!await semaphore.WaitAsync(PlaybackCompletionTimeout, cancellationToken))
                {
                    Log.ForContext<PlaySoundJob>().Warning("Sound notification playback did not report completion within {Timeout}s (deviceId: {DeviceId}); abandoning it.", (int)PlaybackCompletionTimeout.TotalSeconds, DisplayDeviceId);
                    player.Stop();
                }
            }
            finally
            {
                player.PlaybackStopped -= OnPlaybackStoppedHandler;
            }
        }
        catch (Exception ex) when (IsExpectedDeviceException(ex))
        {
            // The endpoint vanished or was reconfigured between resolution and playback
            // (unplugged, disabled, device switch, headphone/USB/BT reconnect, exclusive-mode
            // takeover). That is expected background churn for a notification sound, not a
            // failure: skip the sound at Warning level instead of reporting an error
            // (Sentry SOUNDSWITCH-4A8 and related fingerprints — issue #2557).
            Log.ForContext<PlaySoundJob>().Warning(ex, "Audio device unavailable during notification playback (deviceId: {DeviceId}); skipping the sound: {Message}", DisplayDeviceId, ex.Message);
        }
    }

    /// <summary>
    /// Logs a playback failure raised through <c>PlaybackStopped</c>: device disconnect/reconfiguration
    /// is expected churn and goes to Warning (see issue #2557), while anything else is a real playback
    /// failure that must surface as an error so it reaches Sentry (see issue #2384: a silent WASAPI render
    /// failure shipped unnoticed because this was Warning).
    /// </summary>
    private void ReportPlaybackStoppedFailure(Exception exception)
    {
        var logger = Log.ForContext<PlaySoundJob>();
        if (IsExpectedDeviceException(exception))
        {
            logger.Warning(exception, "Sound notification playback interrupted by audio device disconnect or reconfiguration (deviceId: {DeviceId}).", DisplayDeviceId);
        }
        else
        {
            logger.Error(exception, "Sound notification playback stopped with an error (deviceId: {DeviceId})", DisplayDeviceId);
        }
    }

    /// <summary>
    /// Was the failure just the audio endpoint disappearing or being reconfigured (an expected
    /// condition for a notification sound), rather than a genuine playback defect?
    /// Exposed for tests.
    /// </summary>
    internal static bool IsExpectedDeviceException(Exception exception)
    {
        return exception is AudioDeviceDisconnectedException
            or AudioDeviceInUseException
            or AudioFormatNotSupportedException
            or AudioExclusiveModeNotAllowedException;
    }

    private MMDevice GetDevice(MMDeviceEnumerator enumerator)
    {
        if (string.IsNullOrEmpty(deviceId))
            return TryGetDefaultAudioEndpoint(enumerator);

        try
        {
            return enumerator.GetDevice(deviceId);
        }
        catch (CoreAudioException)
        {
            // The configured device was unplugged or no longer exists: fall back to the default
            // render endpoint so the notification still plays (NAudio throws rather than returning
            // null for a missing device ID).
            Log.ForContext<PlaySoundJob>().Warning("Configured audio device no longer exists (ID: {DeviceId}); falling back to default render endpoint.", deviceId);
            return TryGetDefaultAudioEndpoint(enumerator);
        }
    }

    /// <summary>
    /// Resolves the current default render endpoint, without throwing when none exists
    /// (e.g. playback devices disabled/unplugged): returns <c>null</c> instead so the caller
    /// logs and skips the sound.
    /// </summary>
    private static MMDevice TryGetDefaultAudioEndpoint(MMDeviceEnumerator enumerator)
    {
        enumerator.TryGetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia, out var device);
        return device;
    }

    private IWavePlayer CreatePlayer(MMDevice device)
    {
        if (device == null)
        {
            return new WasapiPlayerBuilder().Build();
        }

        try
        {
            return new WasapiPlayerBuilder().WithDevice(device).WithSharedMode().WithEventSync().WithLatency(200).Build();
        }
        catch (Exception ex)
        {
            // A disconnect/reconfiguration failure logged as Error is what creates the Sentry
            // noise (issue #2557): at Warning level it stays observable without firing alerts.
            Log.ForContext<PlaySoundJob>().Write(IsExpectedDeviceException(ex) ? LogEventLevel.Warning : LogEventLevel.Error,
                ex, "Failed to initialize WasapiPlayer with specified device (deviceId: {DeviceId}).", DisplayDeviceId);
            return new WasapiPlayerBuilder().Build();
        }
    }

    public Task OnFailure(JobException exception)
    {
        if (exception.InnerException is OperationCanceledException)
        {
            return Task.CompletedTask;
        }

        // Real (non-cancellation) playback failures must surface as errors so they reach Sentry
        // (see issue #2384: a silent WASAPI render failure shipped unnoticed because this was Warning).
        Log.ForContext<PlaySoundJob>().Error(exception.InnerException ?? (Exception)exception, "Failed to play sound notification (deviceId: {DeviceId})", DisplayDeviceId);
        return Task.CompletedTask;
    }

    public IRetryAction FailRule { get; } = new NoRetry();
    public TimeSpan? MaxRuntime => TimeSpan.FromSeconds(30);
}
