using Axerrio.BB.DDD.EntityFrameworkCore.Infrastructure.Abstractions;
using Axerrio.BB.DDD.Infrastructure.ExecutionStrategy.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProcessingQueue.Domain.Aggregates.ProcessingQueueItemAggregate;
using ProcessingQueue.Infrastructure.Abstractions;
using ProcessingQueue.Infrastructure.EntityTypeConfigurations;

namespace ProcessingQueue.Infrastructure
{
    public class ProcessingQueueItemDbContext : DbContextUnitOfWork<ProcessingQueueItemDbContext>, IProcessingQueueItemDbContext
    {
        public const string Schema = "processing";
        public DbSet<ProcessingQueueItem> ProcessingQueueItems { get; set; }

        protected ProcessingQueueItemDbContext(DbContextOptions options) : base(options)
        {
        }

        private ProcessingQueueItemDbContext(DbContextOptions<ProcessingQueueItemDbContext> options) : base(options)
        {
        }

        public ProcessingQueueItemDbContext(DbContextOptions<ProcessingQueueItemDbContext> options, IMediator mediator, ILogger<ProcessingQueueItemDbContext> logger, IDbExecutionStrategyFactory<ProcessingQueueItemDbContext> dbExecutionStrategyFactory) : base(options, mediator, logger, dbExecutionStrategyFactory)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new ProcessingQueueItemEntityTypeConfiguration(Schema));
        }
    }
}