using Axerrio.BB.DDD.Job.BackgroundTasks.TimedJobs.Abstractions;
using Axerrio.BB.DDD.Job.Domain.Aggregates.JobAggregate;
using ProcessingQueue.Infrastructure.Abstractions;
using System.Diagnostics;

namespace Floriday_Buyer.Preprocessor.WorkerService.BackgroundTasks
{
    public class WorkServiceEventTimedJob : ManagedJob
    {
        public WorkServiceEventTimedJob(ILogger logger, IServiceProvider provider, int jobSettingId, string name) : base(logger, provider, jobSettingId, name)
        {
        }

        public override async Task DoWorkAsync(IServiceScope scope, CancellationToken cancellationToken = default)
        {
            try
            {
                var processingService = scope.ServiceProvider.GetRequiredService<IProcessingQueueItemProcessing>();

                var itemsForProcessing = await processingService.GetEventsForPreprocessingAsync(cancellationToken);

                // log number of items

                foreach (var item in itemsForProcessing)
                {
                    // TODO implment real pre processing here!!!
                    bool success = (new Random().Next(10) > 5);

                    if (success)
                        await processingService.MarkEventReadyToProcessAsync(item, cancellationToken);
                    else
                        await processingService.MarkEventSkippedAsync(item, cancellationToken);
                }

                Job.UpdateStatus(JobStatus.Success, $"Successfully Preprocessed {itemsForProcessing.Count()} processing queuitems");
            }
            catch (Exception exc)
            {
                Job.UpdateStatus(JobStatus.Failed, $"Failed to Preprocessed queue items {exc.Demystify()}");
            }
        }
    }
}