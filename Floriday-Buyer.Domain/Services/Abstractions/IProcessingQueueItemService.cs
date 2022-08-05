namespace Floriday_Buyer.Domain.Services.Abstractions
{
    public interface IProcessingQueueItemService
    {
        Task<int> AddAsync<TQueueItem>(string instanceKey, TQueueItem queueItem, CancellationToken cancellationToken = default);
    }
}