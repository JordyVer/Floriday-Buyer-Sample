using Axerrio.BB.DDD.Domain.Abstractions;
using EnsureThat;

namespace Floriday_Buyer.Domain.Aggregates.ProcessingQueueItemAggregate
{
    public class ProcessingQueueItem : Entity<int>, IAggregateRoot
    {
        public int ProcessingQueueItemKey
        {
            get => Identity;
            set => Identity = EnsureArg.IsGt(value, 0, nameof(ProcessingQueueItemKey));
        }

        public ProcessingQueueItemState State { get; set; }

        public int ProcessAttempts { get; set; }
        public DateTime InsertedTimestamp { get; set; }
        public DateTime? ProcessedTimestamp { get; set; }
        public DateTime? FailedTimestamp { get; set; }
        public DateTime? SkippedTimestamp { get; set; }
        public DateTime? WaitingTimestamp { get; set; }
        public int WaitingForProcessingQueueItemId { get; set; }
        public DateTime? ReadyForProcessingTimestamp { get; set; }
        public ProcessingQueueItemEvent ProcessingQueueItemEvent { get; set; }

        public string TenantId { get; set; }
        public string TenantUserId { get; set; }

        protected ProcessingQueueItem()
        {
            ProcessAttempts = 0;
            InsertedTimestamp = DateTime.UtcNow;
            State = ProcessingQueueItemState.Inserted;
        }

        protected ProcessingQueueItem(int tenantId, int tenantUserId, ProcessingQueueItemEvent processingQueueItemevent) : this()
        {
            ProcessingQueueItemEvent = processingQueueItemevent;
            TenantId = tenantId.ToString();
            TenantUserId = tenantUserId.ToString();
        }

        public static ProcessingQueueItem Create<TQueueItem>(int tenantId, int tenantUserId, TQueueItem queueItem, string instanceKey, Guid queueItemId)
        {
            return new(tenantId, tenantUserId, ProcessingQueueItemEvent.Create(queueItem, instanceKey, queueItemId));
        }
    }
}