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
        try
        {
            using var semaphore = new SemaphoreSlim(0);
            MMDevice device = null;
            using var enumerator = new MMDeviceEnumerator();
            if (deviceId != null)
            {
                device = enumerator.GetDevice(deviceId);
            }

            using var player = device == null ? new WasapiOut() : new WasapiOut(device, AudioClientShareMode.Shared, true, 200);
            await using var waveStream = new CachedSoundWaveStream(sound);
            player.Init(waveStream);

            void PlayerOnPlaybackStopped(object o, StoppedEventArgs stoppedEventArgs)
            {
                try
                {
                    semaphore.Release();
                }
                catch (ObjectDisposedException)
                {
                    //Ignored
                    //Already disposed, no need to release anything
                }
            }

            player.PlaybackStopped += PlayerOnPlaybackStopped;
            player.Play();
            await semaphore.WaitAsync(cancellationToken);
            player.PlaybackStopped -= PlayerOnPlaybackStopped;
        }
        catch (TaskCanceledException)
        {
            //Ignored
        }
    }

    public Task OnFailure(JobException exception)
    {
        if (exception.InnerException is OperationCanceledException)
        {
            return Task.CompletedTask;
        }
        Log.ForContext<PlaySoundJob>().Warning(exception, "Can play sound");
        return Task.CompletedTask;
    }

    public IRetryAction FailRule { get; } = new NoRetry();
    public TimeSpan? MaxRuntime => TimeSpan.FromSeconds(30);
}