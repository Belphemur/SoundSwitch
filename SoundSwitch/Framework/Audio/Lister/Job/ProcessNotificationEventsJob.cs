using System;
using System.Threading;
using System.Threading.Tasks;
using Job.Scheduler.Job;
using Job.Scheduler.Job.Action;
using Job.Scheduler.Job.Exception;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.Audio.Lister.Job;

public class ProcessNotificationEventsJob : IRecurringJob
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var events = MMNotificationClient.Instance.GetLastEvents();
        AppModel.Instance.AudioDeviceLister.ProcessDeviceUpdates(events);
        return Task.CompletedTask;
    }

    public Task OnFailure(JobException exception)
    {
        return Task.CompletedTask;
    }

    public IRetryAction FailRule { get; } = new ExponentialDecorrelatedJittedBackoffRetry(5, TimeSpan.FromMilliseconds(50));
    public TimeSpan? MaxRuntime { get; } = TimeSpan.FromSeconds(20);
    public TimeSpan Delay { get; } = TimeSpan.FromMilliseconds(200);
}