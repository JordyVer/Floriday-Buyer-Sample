using ProcessingQueue.Domain.Aggregates.ProcessingQueueItemAggregate;

namespace ProcessingQueue.Infrastructure.Abstractions
{
    public interface ProcessingQueueItemPublisher
    {
        Task PublishAsync(ProcessingQueueItem queueItem, CancellationToken cancellationToken = default);
    }
}