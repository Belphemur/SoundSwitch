using System;
using System.Threading;
using System.Threading.Tasks;
using Job.Scheduler.Job;
using Job.Scheduler.Job.Action;
using Job.Scheduler.Job.Exception;
using Serilog;

namespace SoundSwitch.Framework.Updater.Job;

public class CheckForUpdateOnceJob(UpdateChecker updateChecker) : IJob
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return updateChecker.CheckForUpdate(cancellationToken);
    }

    public Task OnFailure(JobException exception)
    {
        Log.Warning(exception, "Couldn't check for update");
        return Task.CompletedTask;
    }

    public IRetryAction FailRule { get; } = new RetryNTimes(3, TimeSpan.FromMinutes(1));
    public TimeSpan? MaxRuntime { get; }
}