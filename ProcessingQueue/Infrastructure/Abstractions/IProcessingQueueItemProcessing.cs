using ProcessingQueue.Domain.ProcessingQueueItems;

namespace ProcessingQueue.Infrastructure.Abstractions
{
    public interface IProcessingQueueItemProcessing
    {
        Task<IEnumerable<ProcessingQueueItem>> GetEventsForPreprocessingAsync(CancellationToken cancellationToken = default);
        Task MarkEventReadyToProcessAsync(ProcessingQueueItem processingQueueItem, CancellationToken cancellationToken = default);
        Task MarkEventSkippedAsync(ProcessingQueueItem processingQueueItem, CancellationToken cancellationToken = default);
    }
}