using EnsureThat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingQueue.Domain.ProcessingQueueItems;
using ProcessingQueue.Infrastructure.Options;

namespace ProcessingQueue.Infrastructure.EntityTypeConfigurations
{
    public class ProcessingQueueItemEntityTypeConfiguration : IEntityTypeConfiguration<ProcessingQueueItem>
    {
        private readonly string _schema;
        private readonly string _tableName;

        public ProcessingQueueItemEntityTypeConfiguration(ProcessingQueueItemDatabaseOptions options)
        {
            _schema = EnsureArg.IsNotNullOrWhiteSpace(options.Schema, nameof(options.Schema));
            _tableName = EnsureArg.IsNotNullOrWhiteSpace(options.TableName, nameof(options.TableName));
        }

        public void Configure(EntityTypeBuilder<ProcessingQueueItem> builder)
        {
            builder.ToTable(_tableName, _schema);
            builder.HasKey(pqi => pqi.ProcessingQueueItemKey);

            builder.Property(pqi => pqi.Message).IsRequired(false);
        }
    }
}