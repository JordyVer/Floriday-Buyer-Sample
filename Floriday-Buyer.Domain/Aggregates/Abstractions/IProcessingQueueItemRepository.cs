using Axerrio.BB.DDD.Domain.Abstractions;
using Axerrio.BB.DDD.Infrastructure.ExecutionStrategy.Abstractions;
using Floriday_Buyer.Domain.Aggregates.ProcessingQueueItemAggregate;
using Microsoft.EntityFrameworkCore.Storage;

namespace Floriday_Buyer.Domain.Aggregates.Abstractions
{
    public interface IProcessingQueueItemRepository : IRepository<ProcessingQueueItem, IDbContextTransaction, IDbExecutionStrategy>
    {
    }
}