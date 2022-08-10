namespace ProcessingQueue.Infrastructure.Abstractions
{
    public interface IProcessingQueueItemCleanup
    {
        Task CleanupEventsAsync(CancellationToken cancellationToken = default);
    }
}