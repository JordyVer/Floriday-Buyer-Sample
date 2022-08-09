using ProcessingQueue.Domain.ProcessingQueueItems;

namespace ProcessingQueue.Infrastructure.Abstractions
{
    public interface IProcessingQueueItemConsumer
    {
        Task<IEnumerable<ProcessingQueueItem>> GetEventsToProcessAsync(CancellationToken cancellationToken = default);

        Task MarkEventFailedAsync(ProcessingQueueItem processingQueueItem, string message, CancellationToken cancellationToken = default);

        Task MarkEventProcessedAsync(ProcessingQueueItem processingQueueItem, CancellationToken cancellationToken = default);
    }
}