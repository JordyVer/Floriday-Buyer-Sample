using Axerrio.BB.DDD.Domain.Abstractions;
using EnsureThat;
using System.Text.Json;

namespace ProcessingQueue.Domain.Aggregates.ProcessingQueueItemAggregate
{
    public class ProcessingQueueItem : Entity<int>, IAggregateRoot
    {
        public int ProcessingQueueItemKey
        {
            get => Identity;
            set => Identity = EnsureArg.IsGt(value, 0, nameof(ProcessingQueueItemKey));
        }

        public ProcessingQueueItemState State { get; private set; }

        public int ProcessAttempts { get; private set; }
        public DateTime InsertedTimestamp { get; private set; }
        public DateTime? ProcessedTimestamp { get; private set; }
        public DateTime? FailedTimestamp { get; private set; }
        public DateTime? SkippedTimestamp { get; private set; }
        public DateTime? WaitingTimestamp { get; private set; }
        public int WaitingForProcessingQueueItemId { get; private set; }
        public DateTime? ReadyForProcessingTimestamp { get; private set; }

        public Guid EventId { get; private set; }
        public string EventInstanceKey { get; private set; }
        public string EventName { get; private set; }
        public string EventTypeName { get; private set; }
        public DateTime EventCreationTimestamp { get; private set; }
        public string EventContent { get; private set; }

        public string TenantId { get; private set; }
        public string TenantUserId { get; private set; }

        protected ProcessingQueueItem()
        {
            ProcessAttempts = 0;
            InsertedTimestamp = DateTime.UtcNow;
            State = ProcessingQueueItemState.Inserted;
        }

        protected ProcessingQueueItem(int tenantId, int tenantUserId, Guid eventId, string eventInstanceKey, string eventName, string eventTypeName, string eventContent)
            : this()
        {
            TenantId = tenantId.ToString();
            TenantUserId = tenantUserId.ToString();
            EventId = eventId;
            EventInstanceKey = eventInstanceKey;
            EventName = eventName;
            EventTypeName = eventTypeName;
            EventCreationTimestamp = DateTime.UtcNow;
            EventContent = eventContent;
        }

        public static ProcessingQueueItem Create<TQueueItem>(string instanceKey, int tenantId, int tenantUserId, TQueueItem queueItem, Guid eventId)
        {
            if (eventId == default) eventId = Guid.NewGuid();
            var eventName = typeof(TQueueItem).Name;
            var eventTypeName = typeof(TQueueItem).FullName;
            var eventContent = JsonSerializer.Serialize(queueItem);

            return new(tenantId, tenantUserId, eventId, instanceKey, eventName, eventTypeName, eventContent);
        }
    }
}