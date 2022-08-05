namespace Floriday_Buyer.Domain.Aggregates.ProcessingQueueItemAggregate
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