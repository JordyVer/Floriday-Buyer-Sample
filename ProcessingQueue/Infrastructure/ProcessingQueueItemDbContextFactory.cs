using Axerrio.BB.DDD.Infrastructure.Idempotency.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ProcessingQueue.Infrastructure.Options;

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

            var options = new ProcessingQueueItemDatabaseOptions()
            {
                Schema = "processing",
                TableName = "ProcessingQueueItem"
            };

            return new ProcessingQueueItemDbContext(optionsBuilder.Options, Microsoft.Extensions.Options.Options.Create(options));
        }
    }
}