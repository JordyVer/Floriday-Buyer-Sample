using Axerrio.BB.DDD.Domain.Abstractions;
using System.Text.Json;

namespace Floriday_Buyer.Domain.Aggregates.ProcessingQueueItemAggregate
{
    public class ProcessingQueueItemEvent : Entity<int>
    {
        public Guid EventId { get; private set; }
        public string EventInstanceKey { get; private set; }
        public string EventName { get; private set; }
        public string EventTypeName { get; private set; }
        public DateTime EventCreationTimestamp { get; private set; }
        public string EventContent { get; private set; }

        private ProcessingQueueItemEvent()
        { }

        private ProcessingQueueItemEvent(Guid eventId, string eventInstanceKey, string eventName, string eventTypeName, string eventContent)
        {
            EventId = eventId;
            EventInstanceKey = eventInstanceKey;
            EventName = eventName;
            EventTypeName = eventTypeName;
            EventCreationTimestamp = DateTime.UtcNow;
            EventContent = eventContent;
        }

        public static ProcessingQueueItemEvent Create<TEvent>(TEvent @event, string eventInstanceKey, Guid eventId = default)
        {
            if (eventId == default) eventId = Guid.NewGuid();
            var eventName = typeof(TEvent).Name;
            var eventTypeName = typeof(TEvent).FullName;
            var eventContent = JsonSerializer.Serialize(@event);

            return new ProcessingQueueItemEvent(eventId, eventInstanceKey, eventName, eventTypeName, eventContent);
        }
    }
}