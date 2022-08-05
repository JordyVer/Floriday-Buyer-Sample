namespace ProcessingQueue.Domain.ProcessingQueueItems
{
    public enum ProcessingQueueItemState
    {
        Inserted,
        Preprocessing,
        ReadyToProcess,
        Processing,
        Failed,
        Skipped,
        Notified,
        Processed
    }
}