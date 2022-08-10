namespace ProcessingQueue.Infrastructure.Options
{
    public class ProcessingQueueItemCleanupOptions
    {
        public int MaxDaysOldEventsGeneral { get; set; } = 3;
        public int MaxDaysEventsFailed { get; set; } = 30;
    }
}