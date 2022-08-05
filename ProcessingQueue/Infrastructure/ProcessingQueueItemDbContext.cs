using Axerrio.BB.DDD.EntityFrameworkCore.Infrastructure.Abstractions;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProcessingQueue.Domain.ProcessingQueueItems;
using ProcessingQueue.Infrastructure.EntityTypeConfigurations;
using ProcessingQueue.Infrastructure.Options;

namespace ProcessingQueue.Infrastructure
{
    public class ProcessingQueueItemDbContext : DbContextUnitOfWork<ProcessingQueueItemDbContext>
    {
        public const string Schema = "processing";
        public DbSet<ProcessingQueueItem> ProcessingQueueItems { get; set; }
        public ProcessingQueueItemDatabaseOptions ProcessingQueueItemDatabaseOptions { get; }

        private ProcessingQueueItemDbContext(DbContextOptions<ProcessingQueueItemDbContext> options) : base(options)
        {
        }

        public ProcessingQueueItemDbContext(DbContextOptions<ProcessingQueueItemDbContext> options, IOptions<ProcessingQueueItemDatabaseOptions> databaseOptions) : base(options)
        {
            ProcessingQueueItemDatabaseOptions = EnsureArg.IsNotNull(databaseOptions, nameof(databaseOptions)).Value;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new ProcessingQueueItemEntityTypeConfiguration(ProcessingQueueItemDatabaseOptions));
        }
    }
}