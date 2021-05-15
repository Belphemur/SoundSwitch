using System;
using System.Threading;
using System.Threading.Tasks;
using Job.Scheduler.Job;
using Job.Scheduler.Job.Action;
using Job.Scheduler.Job.Exception;
using Serilog;
using SoundSwitch.Framework.Configuration;

namespace SoundSwitch.Framework.Updater.Job
{
    public class CheckForUpdateRecurringJob : IRecurringJob
    {
        private readonly UpdateChecker _updateChecker;

        public CheckForUpdateRecurringJob(UpdateChecker updateChecker)
        {
            _updateChecker = updateChecker;
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (AppConfigs.Configuration.UpdateMode == UpdateMode.Never)
            {
                return Task.CompletedTask;
            }

            return _updateChecker.CheckForUpdate(cancellationToken);
        }

        public Task OnFailure(JobException exception)
        {
            Log.Warning(exception, "Couldn't check for update");
            return Task.CompletedTask;
        }

        public IRetryAction FailRule { get; } = new AlwaysRetry(TimeSpan.FromMinutes(30));
        public TimeSpan? MaxRuntime { get; }
        public TimeSpan Delay { get; } = TimeSpan.FromSeconds(AppConfigs.Configuration.UpdateCheckInterval);
    }
}