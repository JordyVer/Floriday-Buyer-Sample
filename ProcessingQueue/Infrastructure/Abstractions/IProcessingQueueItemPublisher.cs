namespace ProcessingQueue.Infrastructure.Abstractions
{
    public interface IProcessingQueueItemPublisher<TResolveContext>
    {
        Task PublishAsync<TQueueItem>(TResolveContext context, string entityName, string instanceKey, TQueueItem queueItem, Guid queueItemId, CancellationToken cancellationToken = default);
    }
}