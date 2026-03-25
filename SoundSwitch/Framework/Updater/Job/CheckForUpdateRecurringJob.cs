using Job.Scheduler.Job.Action;
using Job.Scheduler.Job.Exception;
using Job.Scheduler.Job;
using Serilog;
using SoundSwitch.Framework.Configuration;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace SoundSwitch.Framework.Updater.Job;

public class CheckForUpdateRecurringJob(UpdateChecker updateChecker) : IRecurringJob
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (AppConfigs.Configuration.UpdateMode == UpdateMode.Never)
        {
            return Task.CompletedTask;
        }

        return updateChecker.CheckForUpdate(cancellationToken);
    }

    public Task OnFailure(JobException exception)
    {
        Log.Warning(exception, "Couldn't check for update");
        return Task.CompletedTask;
    }

    public IRetryAction FailRule { get; } = new ExponentialBackoffRetry(TimeSpan.FromMinutes(20), null);
    public TimeSpan? MaxRuntime { get; }
    public TimeSpan Delay { get; } = TimeSpan.FromSeconds(AppConfigs.Configuration.UpdateCheckInterval);
}
