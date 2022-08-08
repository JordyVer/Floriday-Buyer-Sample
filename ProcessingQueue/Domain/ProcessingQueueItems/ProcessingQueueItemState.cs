namespace ProcessingQueue.Domain.ProcessingQueueItems
{
    public enum ProcessingQueueItemState
    {
        Inserted,
        Preprocessing,
        Skipped,
        ReadyToProcess,
        Processing,
        Notified,
        Failed,
        Processed
    }
}