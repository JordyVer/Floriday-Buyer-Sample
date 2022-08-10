using Axerrio.BB.DDD.Infrastructure.ExecutionStrategy.Abstractions;
using Axerrio.BB.DDD.Infrastructure.IntegrationEvents.Options;
using Dapper;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProcessingQueue.Domain.ProcessingQueueItems;
using ProcessingQueue.Infrastructure.Abstractions;
using ProcessingQueue.Infrastructure.Options;
using System.Data.SqlClient;

namespace ProcessingQueue.Infrastructure
{
    public class ProcessingQueueItemCleanup : IProcessingQueueItemCleanup
    {
        private readonly ProcessingQueueItemCleanupOptions _cleanupOptions;
        private readonly SingleDbConnectionOptions _singleDbConnectionOptions;
        private readonly ProcessingQueueItemDatabaseOptions _processingQueueItemDatabaseOptions;
        private readonly ILogger<ProcessingQueueItemCleanup> _logger;
        private readonly IDbExecutionStrategy<ProcessingQueueItemCleanup> _executionStrategy;

        public ProcessingQueueItemCleanup(IOptions<ProcessingQueueItemCleanupOptions> cleanupOptions,
            IOptions<SingleDbConnectionOptions> singleDbConnectionOptions,
            IOptions<ProcessingQueueItemDatabaseOptions> processingQueueItemDatabaseOptions,
            ILogger<ProcessingQueueItemCleanup> logger,
            IDbExecutionStrategy<ProcessingQueueItemCleanup> executionStrategy)
        {
            _cleanupOptions = EnsureArg.IsNotNull(cleanupOptions, nameof(cleanupOptions)).Value;
            _singleDbConnectionOptions = EnsureArg.IsNotNull(singleDbConnectionOptions, nameof(singleDbConnectionOptions)).Value;
            _processingQueueItemDatabaseOptions = EnsureArg.IsNotNull(processingQueueItemDatabaseOptions, nameof(processingQueueItemDatabaseOptions)).Value;
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
            _executionStrategy = EnsureArg.IsNotNull(executionStrategy, nameof(executionStrategy));
        }

        public Task CleanupEventsAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug($"Cleanup for processing queue items, was cancelled");
                return Task.CompletedTask;
            }

            return _executionStrategy.ExecuteAsync(CleanupEvents(), cancellationToken);
        }

        private Func<CancellationToken, Task> CleanupEvents()
        {
            return async (cancellationToken) =>
            {
                _logger.LogDebug($"Cleanup processing queueItems started");
                using var connection = new SqlConnection(_singleDbConnectionOptions.ConnectionString);
                connection.Open();
                var param = new
                {
                    OldestFailedDateTime = DateTime.UtcNow.AddDays(-_cleanupOptions.MaxDaysEventsFailed),
                    OldestGeneralDateTime = DateTime.UtcNow.AddDays(-_cleanupOptions.MaxDaysEventsFailed)
                };

                await connection.ExecuteAsync(CleanupEventsSql, param);
            };
        }

        private string CleanupEventsSql
        {
            get
            {
                return $@"delete from {_processingQueueItemDatabaseOptions.Schema}.{_processingQueueItemDatabaseOptions.TableName}
                        where ([State] = {(int)ProcessingQueueItemState.Failed} and [FailedTimestamp] < @OldestFailedDateTime)
                        and ([FailedTimestamp] is null and [InsertedTimestamp] < @OldestGeneralDateTime)";
            }
        }
    }
}