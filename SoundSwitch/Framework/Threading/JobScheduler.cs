using Job.Scheduler.Builder;
using Job.Scheduler.Scheduler;

namespace SoundSwitch.Framework.Threading
{
    public static class JobScheduler
    {
        public static readonly IJobScheduler Instance = new Job.Scheduler.Scheduler.JobScheduler(new JobRunnerBuilder());
    }
}