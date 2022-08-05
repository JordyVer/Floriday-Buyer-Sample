using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingQueue.Domain.Aggregates.ProcessingQueueItemAggregate;

namespace ProcessingQueue.Infrastructure.EntityTypeConfigurations
{
    public class ProcessingQueueItemEntityTypeConfiguration : IEntityTypeConfiguration<ProcessingQueueItem>
    {
        private readonly string _schema;

        public ProcessingQueueItemEntityTypeConfiguration(string schema = "")
        {
            _schema = schema;
        }

        public void Configure(EntityTypeBuilder<ProcessingQueueItem> builder)
        {
            builder.ToTable("ProcessingQueueItem", _schema);
            builder.HasKey(pqi => pqi.ProcessingQueueItemKey);
            builder.Ignore(pqi => pqi.Identity);
        }
    }
}