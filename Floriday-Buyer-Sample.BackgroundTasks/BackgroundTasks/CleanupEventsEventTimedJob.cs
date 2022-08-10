using Axerrio.BB.DDD.Job.BackgroundTasks.TimedJobs.Abstractions;
using Axerrio.BB.DDD.Job.Domain.Aggregates.JobAggregate;
using ProcessingQueue.Infrastructure.Abstractions;
using System.Diagnostics;

namespace Floriday_Buyer_Sample.BackgroundTasks.BackgroundTasks
{
    public class CleanupEventsEventTimedJob : ManagedJob
    {
        public CleanupEventsEventTimedJob(ILogger<CleanupEventsEventTimedJob> logger, IServiceProvider provider, int jobSettingId)
            : base(logger, provider, jobSettingId, nameof(CleanupEventsEventTimedJob))
        {
        }

        public override async Task DoWorkAsync(IServiceScope scope, CancellationToken cancellationToken = default)
        {
            var cleanupService = scope.ServiceProvider.GetRequiredService<IProcessingQueueItemCleanup>();
            try
            {
                await cleanupService.CleanupEventsAsync(cancellationToken);

                Job.UpdateStatus(JobStatus.Success, $"Successfully cleaned up all events");
            }
            catch (Exception exc)
            {
                Job.UpdateStatus(JobStatus.Failed, $"Failed to cleanup queue items {exc.Demystify()}");
            }
        }
    }
}