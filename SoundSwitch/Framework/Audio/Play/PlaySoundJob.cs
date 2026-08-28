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

namespace SoundSwitch.Framework.Audio.Play;

public class PlaySoundJob([CanBeNull] string deviceId, [NotNull] CachedSound sound) : IJob
{
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
        using var device = GetDevice(enumerator);
        if (device == null)
        {
            Log.ForContext<PlaySoundJob>().Warning("No audio device found for specified ID: {DeviceId}.", string.IsNullOrEmpty(deviceId) ? "<default>" : deviceId);
            return;
        }

        using var player = CreatePlayer(device);
        await using var waveStream = new CachedSoundWaveStream(sound);

        player.Init(waveStream);

        void OnPlaybackStoppedHandler(object o, StoppedEventArgs stoppedEventArgs)
        {
            if (stoppedEventArgs.Exception != null)
            {
                // Real (non-cancellation) playback failures must surface as errors so they reach Sentry
                // (see issue #2384: a silent WASAPI render failure shipped unnoticed because this was Warning).
                Log.ForContext<PlaySoundJob>().Error(stoppedEventArgs.Exception, "Sound notification playback stopped with an error (deviceId: {DeviceId})", string.IsNullOrEmpty(deviceId) ? "<default>" : deviceId);
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
            await semaphore.WaitAsync(cancellationToken);
        }
        finally
        {
            player.PlaybackStopped -= OnPlaybackStoppedHandler;
        }
    }

    private MMDevice GetDevice(MMDeviceEnumerator enumerator)
    {
        if (string.IsNullOrEmpty(deviceId))
            return enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

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
            return enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }
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
            Log.ForContext<PlaySoundJob>().Error(ex, "Failed to initialize WasapiPlayer with specified device.");
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
        Log.ForContext<PlaySoundJob>().Error(exception.InnerException ?? (Exception)exception, "Failed to play sound notification (deviceId: {DeviceId})", string.IsNullOrEmpty(deviceId) ? "<default>" : deviceId);
        return Task.CompletedTask;
    }

    public IRetryAction FailRule { get; } = new NoRetry();
    public TimeSpan? MaxRuntime => TimeSpan.FromSeconds(30);
}
