namespace ProcessingQueue.Infrastructure.Abstractions
{
    public interface IProcessingQueueItemPublisher
    {
        Task PublishAsync<TQueueItem>(string instanceKey, TQueueItem queueItem, Guid queueItemId, CancellationToken cancellationToken = default);
    }
}