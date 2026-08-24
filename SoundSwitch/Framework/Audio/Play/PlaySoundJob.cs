using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Job.Scheduler.Job;
using Job.Scheduler.Job.Action;
using Job.Scheduler.Job.Exception;

using Serilog;

using SoundSwitch.Audio.Manager.Playback;

namespace SoundSwitch.Framework.Audio.Play;

public class PlaySoundJob([CanBeNull] string deviceId, [NotNull] CachedSound sound) : IJob
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (sound == null)
            throw new ArgumentNullException(nameof(sound));

        try
        {
            await SoundPlayer.PlayAsync(sound.AudioData, sound.WaveFormat, deviceId, cancellationToken, OnPlaybackStopped);
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

    private static void OnPlaybackStopped(Exception exception)
    {
        if (exception != null)
        {
            Log.ForContext<PlaySoundJob>().Warning(exception, "Sound notification playback stopped with an error");
        }
    }

    public Task OnFailure(JobException exception)
    {
        if (exception.InnerException is OperationCanceledException)
        {
            return Task.CompletedTask;
        }

        Log.ForContext<PlaySoundJob>().Warning(exception.InnerException ?? (Exception)exception, "Failed to play sound notification");
        return Task.CompletedTask;
    }

    public IRetryAction FailRule { get; } = new NoRetry();
    public TimeSpan? MaxRuntime => TimeSpan.FromSeconds(30);
}
