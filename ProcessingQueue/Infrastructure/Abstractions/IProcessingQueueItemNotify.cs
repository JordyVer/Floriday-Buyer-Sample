using ProcessingQueue.Domain.ProcessingQueueItems;

namespace ProcessingQueue.Infrastructure.Abstractions
{
    public interface IProcessingQueueItemNotify
    {
        Task<IEnumerable<ProcessingQueueItem>> GetAllEventsToNotifyAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<ProcessingQueueItem>> GetEventsToNotifyAsync(int tenantId, CancellationToken cancellationToken = default);
    }
}