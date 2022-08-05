namespace ProcessingQueue.Domain.Aggregates.ProcessingQueueItemAggregate
{
    public enum ProcessingQueueItemState
    {
        Inserted,
        Preprocessing,
        Processing,
        Failed,
        Skipped,
        Processed
    }
}