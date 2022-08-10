using Axerrio.BB.DDD.Job.BackgroundTasks.TimedJobs.Abstractions;
using Axerrio.BB.DDD.Job.Domain.Aggregates.JobAggregate;
using ProcessingQueue.Domain.ProcessingQueueItems;
using ProcessingQueue.Infrastructure.Abstractions;
using System.Diagnostics;

namespace Floriday_Buyer_Sample.BackgroundTasks.BackgroundTasks
{
    public class NotifyFailedEventTimedJob : ManagedJob
    {
        public NotifyFailedEventTimedJob(ILogger<NotifyFailedEventTimedJob> logger, IServiceProvider provider, int jobSettingId) : base(logger, provider, jobSettingId, nameof(NotifyFailedEventTimedJob))
        {
        }

        public override async Task DoWorkAsync(IServiceScope scope, CancellationToken cancellationToken = default)
        {
            var notifyEventsService = scope.ServiceProvider.GetRequiredService<IProcessingQueueItemNotify>();
            try
            {
                var tenantIds = new List<int> { 1, 2, 3, 4, 5 };
                foreach (var tenantId in tenantIds)
                {
                    // get tenantInfo
                    var eventsToNotify = await notifyEventsService.GetEventsToNotifyAsync(tenantId, cancellationToken);
                    foreach (var item in eventsToNotify)
                    {
                        NotifyEvent(tenantId, item);
                    }
                }
                Job.UpdateStatus(JobStatus.Success, $"Successfully sent notifications for all tenants");
            }
            catch (Exception exc)
            {
                Job.UpdateStatus(JobStatus.Failed, $"Failed to sent notifications {exc.Demystify()}");
            }
        }

        private void NotifyEvent(int tenantId, ProcessingQueueItem item)
        {
            _logger.LogInformation($"Notification for tenant {tenantId}, {item.EventName} with content: {item.EventContent}.");
        }
    }
}