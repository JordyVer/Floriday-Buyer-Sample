using Axerrio.BB.DDD.EntityFrameworkCore.Infrastructure.ExecutionStrategy;
using Axerrio.BB.DDD.Infrastructure.ExecutionStrategy.Abstractions;
using Axerrio.BB.DDD.Infrastructure.Idempotency.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;

namespace ProcessingQueue.Infrastructure
{
    public class ProcessingQueueItemDbContextFactory : IDesignTimeDbContextFactory<ProcessingQueueItemDbContext>
    {
        public ProcessingQueueItemDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProcessingQueueItemDbContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLOCALDB;Initial Catalog=DataStore;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

            var schema = "Purchase";
            var _databaseOptions = new DatabaseOptions()
            {
                Schema = schema
            };

            var mediator = new Mediator(null);

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddConsole()
                    .AddEventLog();
            });

            var logger = loggerFactory.CreateLogger<ProcessingQueueItemDbContext>();

            IDbExecutionStrategyFactory<ProcessingQueueItemDbContext> factory = new DbContextExecutionStrategyFactory<ProcessingQueueItemDbContext>();

            return new ProcessingQueueItemDbContext(optionsBuilder.Options, mediator, logger, factory);
        }
    }
}