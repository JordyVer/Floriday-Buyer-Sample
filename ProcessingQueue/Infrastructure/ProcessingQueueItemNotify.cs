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
    public class ProcessingQueueItemNotify : IProcessingQueueItemNotify
    {
        private readonly ProcessingQueueItemCleanupOptions _cleanupOptions;
        private readonly SingleDbConnectionOptions _singleDbConnectionOptions;
        private readonly ProcessingQueueItemDatabaseOptions _processingQueueItemDatabaseOptions;
        private readonly ILogger<ProcessingQueueItemCleanup> _logger;
        private readonly IDbExecutionStrategy<ProcessingQueueItemCleanup> _executionStrategy;

        public ProcessingQueueItemNotify(IOptions<ProcessingQueueItemCleanupOptions> cleanupOptions,
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

        public Task<IEnumerable<ProcessingQueueItem>> GetAllEventsToNotifyAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug($"Cleanup for processing queue items, was cancelled");
                return Task.FromResult(Enumerable.Empty<ProcessingQueueItem>());
            }

            return _executionStrategy.ExecuteAsync(GetAllEventsToNotify(), cancellationToken);
        }

        private Func<CancellationToken, Task<IEnumerable<ProcessingQueueItem>>> GetAllEventsToNotify()
        {
            return async (cancellationToken) =>
            {
                _logger.LogDebug($"Cleanup processing queueItems started");
                using var connection = new SqlConnection(_singleDbConnectionOptions.ConnectionString);
                connection.Open();

                return await connection.QueryAsync<ProcessingQueueItem>(AllEventsToNotifySql);
            };
        }

        public Task<IEnumerable<ProcessingQueueItem>> GetEventsToNotifyAsync(int tenantId, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug($"Cleanup for processing queue items, was cancelled");
                return Task.FromResult(Enumerable.Empty<ProcessingQueueItem>());
            }

            return _executionStrategy.ExecuteAsync(GetEventsToNotify(tenantId), cancellationToken);
        }

        private Func<CancellationToken, Task<IEnumerable<ProcessingQueueItem>>> GetEventsToNotify(int tenantId)
        {
            return async (cancellationToken) =>
            {
                _logger.LogDebug($"Cleanup processing queueItems started");
                using var connection = new SqlConnection(_singleDbConnectionOptions.ConnectionString);
                connection.Open();
                var param = new { tenantId };

                return await connection.QueryAsync<ProcessingQueueItem>(EventsToNotifySql, param);
            };
        }

        private string AllEventsToNotifySql
        {
            get
            {
                return $@"with eqi as
                    (
                        select q.*
                        from {_processingQueueItemDatabaseOptions.Schema}.{_processingQueueItemDatabaseOptions.TableName} as q with (rowlock, readpast)
                        where q.[State] = {(int)ProcessingQueueItemState.Failed} or q.[State] = {(int)ProcessingQueueItemState.Skipped}
                    )
                    update eqi set eqi.[State] = {(int)ProcessingQueueItemState.Notified}
                    output inserted.*";
            }
        }

        private string EventsToNotifySql
        {
            get
            {
                return $@"with eqi as
                    (
                        select q.*
                        from {_processingQueueItemDatabaseOptions.Schema}.{_processingQueueItemDatabaseOptions.TableName} as q with (rowlock, readpast)
                        where q.[State] = {(int)ProcessingQueueItemState.Failed} or q.[State] = {(int)ProcessingQueueItemState.Skipped}
                        and q.TenantId = @tenantId
                    )
                    update eqi set eqi.[State] = {(int)ProcessingQueueItemState.Notified}
                    output inserted.*";
            }
        }
    }
}