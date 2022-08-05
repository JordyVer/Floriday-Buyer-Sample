using Axerrio.BB.DDD.Job.BackgroundTasks.TimedJobs.Abstractions;

namespace Floriday_Buyer.Preprocessor.WorkerService.BackgroundTasks
{
    public class WorkServiceEventTimedJob : ManagedJob
    {
        public WorkServiceEventTimedJob(ILogger logger, IServiceProvider provider, int jobSettingId, string name) : base(logger, provider, jobSettingId, name)
        {
        }

        public override Task DoWorkAsync(IServiceScope scope, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}