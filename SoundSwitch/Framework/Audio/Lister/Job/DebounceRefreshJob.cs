using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Job.Scheduler.Job;
using Job.Scheduler.Job.Action;
using Job.Scheduler.Job.Exception;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.Audio.Lister.Job;

public class DebounceRefreshJob : IDebounceJob
{
    private readonly IAudioDeviceLister _lister;
    private readonly ILogger _logger;

    public DebounceRefreshJob(DeviceState state, IAudioDeviceLister lister, ILogger logger)
    {
        _lister = lister;
        _logger = logger;
        Key = $"Refresh-Device-{state}";
    }

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(() => _lister.Refresh(cancellationToken), cancellationToken);
    }

    public Task OnFailure(JobException exception)
    {
        if (exception.InnerException is OperationCanceledException)
        {
            _logger.Warning("Refresh of device interrupted");
            return Task.CompletedTask;
        }
        
        if(exception.InnerException is AggregateException aggregateException && aggregateException.InnerExceptions.FirstOrDefault() is OperationCanceledException)
        {
            _logger.Warning("Refresh of device interrupted");
            return Task.CompletedTask;
        }

        _logger.Warning(exception, "Can't refresh devices");
        return Task.CompletedTask;
    }

    public IRetryAction FailRule { get; } = new NoRetry();
    public TimeSpan? MaxRuntime { get; } = TimeSpan.FromSeconds(3);
    public string Key { get; }
    public TimeSpan DebounceTime { get; } = TimeSpan.FromMilliseconds(175);
}