using System.Text.Json;

namespace ProcessingQueue.Domain.ProcessingQueueItems
{
    public class ProcessingQueueItem
    {
        public int ProcessingQueueItemKey { get; set; }

        public ProcessingQueueItemState State { get; set; }

        public int ProcessAttempts { get; set; }
        public DateTime InsertedTimestamp { get; set; }
        public DateTime? ProcessedTimestamp { get; set; }
        public DateTime? FailedTimestamp { get; set; }
        public DateTime? SkippedTimestamp { get; set; }
        public DateTime? WaitingTimestamp { get; set; }
        public int WaitingForProcessingQueueItemId { get; set; }
        public DateTime? ReadyForProcessingTimestamp { get; set; }
        public string Message { get; set; }

        public Guid EventId { get; private set; }
        public string EventEntityName { get; private set; }
        public string EventInstanceKey { get; private set; }
        public string EventName { get; private set; }
        public string EventTypeName { get; private set; }
        public DateTime EventCreationTimestamp { get; private set; }
        public string EventContent { get; private set; }

        public string TenantId { get; set; }
        public string TenantUserId { get; set; }

        protected ProcessingQueueItem()
        {
            ProcessAttempts = 0;
            InsertedTimestamp = DateTime.UtcNow;
            State = ProcessingQueueItemState.Inserted;
        }

        protected ProcessingQueueItem(int tenantId, int tenantUserId, Guid eventId, string eventEntityName, string eventInstanceKey,
            string eventName, string eventTypeName, string eventContent) : this()
        {
            TenantId = tenantId.ToString();
            TenantUserId = tenantUserId.ToString();
            EventId = eventId;
            EventEntityName = eventEntityName;
            EventInstanceKey = eventInstanceKey;
            EventName = eventName;
            EventTypeName = eventTypeName;
            EventCreationTimestamp = DateTime.UtcNow;
            EventContent = eventContent;
        }

        public static ProcessingQueueItem Create<TQueueItem>(string eventEntityName, string eventInstanceKey, int tenantId,
            int tenantUserId, TQueueItem queueItem, Guid eventId)
        {
            if (eventId == default) eventId = Guid.NewGuid();
            var eventName = typeof(TQueueItem).Name;
            var eventTypeName = typeof(TQueueItem).FullName;
            var eventContent = JsonSerializer.Serialize(queueItem);

            return new(tenantId, tenantUserId, eventId, eventEntityName, eventInstanceKey, eventName, eventTypeName, eventContent);
        }
    }
}