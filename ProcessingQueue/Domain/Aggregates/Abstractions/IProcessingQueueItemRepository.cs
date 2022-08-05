using Axerrio.BB.DDD.Domain.Abstractions;
using Axerrio.BB.DDD.Infrastructure.ExecutionStrategy.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;
using ProcessingQueue.Domain.Aggregates.ProcessingQueueItemAggregate;

namespace ProcessingQueue.Domain.Aggregates.Abstractions
{
    public interface IProcessingQueueItemRepository : IRepository<ProcessingQueueItem, IDbContextTransaction, IDbExecutionStrategy>
    {
    }
}