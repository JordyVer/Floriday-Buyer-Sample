using Axerrio.BB.DDD.Domain.Multitenancy.Abstractions;
using Axerrio.BB.DDD.Infrastructure.ExecutionStrategy.Abstractions;
using Axerrio.BB.DDD.Infrastructure.Sharding.Options;
using Axerrio.BB.DDD.Sql.Infrastructure.Abstractions;
using Dapper;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProcessingQueue.Domain.ProcessingQueueItems;
using ProcessingQueue.Infrastructure.Abstractions;
using ProcessingQueue.Infrastructure.Options;

namespace ProcessingQueue.Infrastructure
{
    public class ProcessingQueueItemConsumer<TTenant, TTenantUser> : IProcessingQueueItemConsumer
        where TTenant : class, ITenant
        where TTenantUser : TenantUser, ITenantUser
    {
        private readonly IDbExecutionStrategy<ProcessingQueueItemConsumer<TTenant, TTenantUser>> _executionStrategy;
        private readonly ProcessingQueueItemDatabaseOptions _processingQueueItemDatabaseOptions;
        private readonly ShardingOptions _shardingOptions;
        private readonly ITenantContextAccessor<TTenant, TTenantUser> _tenantContextAccessor;
        private readonly IDdrDbConnectionFactory<int> _ddrDbConnectionFactory;
        private readonly ILogger<ProcessingQueueItemConsumer<TTenant, TTenantUser>> _logger;

        public ProcessingQueueItemConsumer(IDbExecutionStrategy<ProcessingQueueItemConsumer<TTenant, TTenantUser>> executionStrategy,
            IOptions<ProcessingQueueItemDatabaseOptions> processingQueueItemDatabaseOptions,
            IOptions<ShardingOptions> shardingOptions,
            ITenantContextAccessor<TTenant, TTenantUser> tenantContextAccessor,
            IDdrDbConnectionFactory<int> ddrDbConnectionFactory,
            ILogger<ProcessingQueueItemConsumer<TTenant, TTenantUser>> logger)
        {
            _executionStrategy = EnsureArg.IsNotNull(executionStrategy, nameof(executionStrategy));
            _processingQueueItemDatabaseOptions = EnsureArg.IsNotNull(processingQueueItemDatabaseOptions, nameof(processingQueueItemDatabaseOptions)).Value;
            _shardingOptions = EnsureArg.IsNotNull(shardingOptions, nameof(shardingOptions)).Value;
            _tenantContextAccessor = EnsureArg.IsNotNull(tenantContextAccessor, nameof(tenantContextAccessor));
            _ddrDbConnectionFactory = EnsureArg.IsNotNull(ddrDbConnectionFactory, nameof(ddrDbConnectionFactory));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
        }

        public Task<IEnumerable<ProcessingQueueItem>> GetEventsToProcessAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug($"Retrieving processing queueItems to process, was cancelled");

                return Task.FromResult(Enumerable.Empty<ProcessingQueueItem>());
            }

            return _executionStrategy.ExecuteAsync(GetToProcess(), cancellationToken);
        }

        private Func<CancellationToken, Task<IEnumerable<ProcessingQueueItem>>> GetToProcess()
        {
            return async (cancellationToken) =>
            {
                _logger.LogDebug($"Retrieving processing queueItems to process");
                using var connection = _ddrDbConnectionFactory.Create(_tenantContextAccessor.TenantContext.Tenant.TenantId, _shardingOptions.ShardMapName, _shardingOptions.ConnectionString);
                var queueItems = await connection.QueryAsync<ProcessingQueueItem>(EventsToProcessSql);
                return queueItems;
            };
        }

        public Task MarkEventProcessedAsync(ProcessingQueueItem processingQueueItem, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug($"Mark Processed processing queueItems after processing, was cancelled");

                return Task.CompletedTask;
            }
            return _executionStrategy.ExecuteAsync(MarkEventProcessed(processingQueueItem), cancellationToken);
        }

        private Func<CancellationToken, Task> MarkEventProcessed(ProcessingQueueItem processingQueueItem)
        {
            return async (cancellationToken) =>
            {
                _logger.LogDebug($"ProcessingQueueItem {processingQueueItem.ProcessingQueueItemKey}, is Processed");

                using var connection = _ddrDbConnectionFactory.Create(_tenantContextAccessor.TenantContext.Tenant.TenantId, _shardingOptions.ShardMapName, _shardingOptions.ConnectionString);
                var param = new { processingQueueItem.ProcessingQueueItemKey, ProcessedTimestamp = DateTime.UtcNow };

                await connection.ExecuteAsync(MarkEventProcessedSql, param);
            };
        }

        // TODO problemdetails op message
        public Task MarkEventFailedAsync(ProcessingQueueItem processingQueueItem, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug($"Mark Failed processing queueItems while processing, was cancelled");

                return Task.CompletedTask;
            }
            return _executionStrategy.ExecuteAsync(MarkEventFailed(processingQueueItem), cancellationToken);
        }

        private Func<CancellationToken, Task> MarkEventFailed(ProcessingQueueItem processingQueueItem)
        {
            return async (cancellationToken) =>
            {
                _logger.LogDebug($"ProcessingQueueItem {processingQueueItem.ProcessingQueueItemKey}, Failed while processing");

                using var connection = _ddrDbConnectionFactory.Create(_tenantContextAccessor.TenantContext.Tenant.TenantId, _shardingOptions.ShardMapName, _shardingOptions.ConnectionString);
                var param = new { processingQueueItem.ProcessingQueueItemKey, FailedTimestamp = DateTime.UtcNow };

                await connection.ExecuteAsync(MarkEventFailedSql, param);
            };
        }

        #region SQL

        private string EventsToProcessSql
        {
            get
            {
                return $@"with eqi as
                    (
                        select top {_processingQueueItemDatabaseOptions.MaxItemsToProcess} q.*
                        from {_processingQueueItemDatabaseOptions.Schema}.{_processingQueueItemDatabaseOptions.TableName} as q with (rowlock, readpast)
                        where q.[State] in ({(int)ProcessingQueueItemState.ReadyToProcess},{(int)ProcessingQueueItemState.Processing},{(int)ProcessingQueueItemState.Failed})
                        and q.[ProcessAttempts] < {_processingQueueItemDatabaseOptions.RetryAttempts}

                    )
                    update eqi set eqi.[State] = {(int)ProcessingQueueItemState.Processing}
                            , eqi.ProcessAttempts = eqi.ProcessAttempts + 1
                    output inserted.*";
            }
        }

        private string MarkEventFailedSql
        {
            get
            {
                return $@"update {_processingQueueItemDatabaseOptions.Schema}.{_processingQueueItemDatabaseOptions.TableName} set [State] = {(int)ProcessingQueueItemState.Failed}
                        ,[FailedTimestamp] = @FailedTimestamp where [ProcessingQueueItemKey] = @ProcessingQueueItemKey";
            }
        }

        private string MarkEventProcessedSql
        {
            get
            {
                return $@"update {_processingQueueItemDatabaseOptions.Schema}.{_processingQueueItemDatabaseOptions.TableName} set [State] = {(int)ProcessingQueueItemState.Processed}
                        ,[ProcessedTimestamp] = @ProcessedTimestamp where [ProcessingQueueItemKey] = @ProcessingQueueItemKey";
            }
        }

        #endregion SQL
    }
}