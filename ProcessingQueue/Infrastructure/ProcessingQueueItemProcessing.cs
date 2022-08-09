using Axerrio.BB.DDD.Domain.Multitenancy.Abstractions;
using Axerrio.BB.DDD.Infrastructure.ExecutionStrategy.Abstractions;
using Axerrio.BB.DDD.Infrastructure.IntegrationEvents.Options;
using Axerrio.BB.DDD.Infrastructure.Sharding.Options;
using Axerrio.BB.DDD.Sql.Infrastructure.Abstractions;
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
    public class ProcessingQueueItemProcessing<TTenant, TTenantUser> : IProcessingQueueItemProcessing
        where TTenant : class, ITenant
        where TTenantUser : TenantUser, ITenantUser
    {
        private readonly IDbExecutionStrategy<ProcessingQueueItemProcessing<TTenant, TTenantUser>> _executionStrategy;
        private readonly ProcessingQueueItemDatabaseOptions _processingQueueItemDatabaseOptions;
        private readonly ShardingOptions _shardingOptions;
        private readonly SingleDbConnectionOptions _singleDbConnectionOptions;
        private readonly ITenantContextAccessor<TTenant, TTenantUser> _tenantContextAccessor;
        private readonly IDdrDbConnectionFactory<int> _ddrDbConnectionFactory;
        private readonly ILogger<ProcessingQueueItemProcessing<TTenant, TTenantUser>> _logger;

        public ProcessingQueueItemProcessing(IDbExecutionStrategy<ProcessingQueueItemProcessing<TTenant, TTenantUser>> executionStrategy,
            IOptions<ProcessingQueueItemDatabaseOptions> processingQueueItemDatabaseOptions,
            IOptions<ShardingOptions> shardingOptions,
            IOptions<SingleDbConnectionOptions> singleDbConnectionOptions,
            ITenantContextAccessor<TTenant, TTenantUser> tenantContextAccessor,
            IDdrDbConnectionFactory<int> ddrDbConnectionFactory,
            ILogger<ProcessingQueueItemProcessing<TTenant, TTenantUser>> logger)
        {
            _executionStrategy = EnsureArg.IsNotNull(executionStrategy, nameof(executionStrategy));
            _processingQueueItemDatabaseOptions = EnsureArg.IsNotNull(processingQueueItemDatabaseOptions, nameof(processingQueueItemDatabaseOptions)).Value;
            _shardingOptions = EnsureArg.IsNotNull(shardingOptions, nameof(shardingOptions)).Value;
            _singleDbConnectionOptions = EnsureArg.IsNotNull(singleDbConnectionOptions, nameof(singleDbConnectionOptions)).Value;
            _tenantContextAccessor = EnsureArg.IsNotNull(tenantContextAccessor, nameof(tenantContextAccessor));
            _ddrDbConnectionFactory = EnsureArg.IsNotNull(ddrDbConnectionFactory, nameof(ddrDbConnectionFactory));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
        }

        public Task<IEnumerable<ProcessingQueueItem>> GetEventsForPreprocessingAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug($"Retrieving processing queueItems for pre-processing, was cancelled");

                return Task.FromResult(Enumerable.Empty<ProcessingQueueItem>());
            }

            return _executionStrategy.ExecuteAsync(GetPreprocessing(), cancellationToken);
        }

        private Func<CancellationToken, Task<IEnumerable<ProcessingQueueItem>>> GetPreprocessing()
        {
            return async (cancellationToken) =>
            {
                _logger.LogDebug($"Retrieving processing queueItems for pre-processing");
                using var connection = new SqlConnection(_singleDbConnectionOptions.ConnectionString);
                connection.Open();
                var queueItems = await connection.QueryAsync<ProcessingQueueItem>(PickupPreprocessSql);
                return queueItems;
            };
        }

        public Task MarkEventSkippedAsync(ProcessingQueueItem processingQueueItem, string message, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug($"Mark Skipped processing queueItems after pre-processing, was cancelled");

                return Task.CompletedTask;
            }
            return _executionStrategy.ExecuteAsync(MarkEventSkipped(processingQueueItem, message), cancellationToken);
        }

        private Func<CancellationToken, Task> MarkEventSkipped(ProcessingQueueItem processingQueueItem, string message)
        {
            return async (cancellationToken) =>
            {
                _logger.LogDebug($"ProcessingQueueItem {processingQueueItem.ProcessingQueueItemKey}, is skipped");

                using var connection = _ddrDbConnectionFactory.Create(_tenantContextAccessor.TenantContext.Tenant.TenantId, _shardingOptions.ShardMapName, _shardingOptions.ConnectionString);
                var param = new { processingQueueItem.ProcessingQueueItemKey, SkippedTimestamp = DateTime.UtcNow, message };

                await connection.ExecuteAsync(MarkEventSkippedSql, param);
            };
        }

        public Task MarkEventReadyToProcessAsync(ProcessingQueueItem processingQueueItem, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug($"Mark ReadyToProcess processing queueItems after pre-processing, was cancelled");

                return Task.CompletedTask;
            }
            return _executionStrategy.ExecuteAsync(MarkEventReadyToProcess(processingQueueItem, ProcessingQueueItemState.ReadyToProcess), cancellationToken);
        }

        private Func<CancellationToken, Task> MarkEventReadyToProcess(ProcessingQueueItem processingQueueItem, ProcessingQueueItemState state)
        {
            return async (cancellationToken) =>
            {
                _logger.LogDebug($"ProcessingQueueItem {processingQueueItem.ProcessingQueueItemKey}, is Ready to Process");

                using var connection = _ddrDbConnectionFactory.Create(_tenantContextAccessor.TenantContext.Tenant.TenantId, _shardingOptions.ShardMapName, _shardingOptions.ConnectionString);
                var param = new { processingQueueItem.ProcessingQueueItemKey, ReadyForProcessingTimestamp = DateTime.UtcNow };

                await connection.ExecuteAsync(MarkEventReadyToProcessSql, param);
            };
        }

        #region SQL

        private string PickupPreprocessSql
        {
            get
            {
                return $@"with eqi as
                    (
                        select top {_processingQueueItemDatabaseOptions.MaxItemsToPreprocess} q.*
                        from {_processingQueueItemDatabaseOptions.Schema}.{_processingQueueItemDatabaseOptions.TableName} as q with (rowlock, readpast)
                        where q.[State] = {(int)ProcessingQueueItemState.Inserted}
                    )
                    update eqi set eqi.[State] = {(int)ProcessingQueueItemState.Preprocessing}
                    output inserted.*";
            }
        }

        private string MarkEventReadyToProcessSql
        {
            get
            {
                return $@"update {_processingQueueItemDatabaseOptions.Schema}.{_processingQueueItemDatabaseOptions.TableName}
                        set [State] = {(int)ProcessingQueueItemState.ReadyToProcess},[ReadyForProcessingTimestamp] = @ReadyForProcessingTimestamp
                        where [ProcessingQueueItemKey] = @ProcessingQueueItemKey";
            }
        }

        private string MarkEventSkippedSql
        {
            get
            {
                return $@"update {_processingQueueItemDatabaseOptions.Schema}.{_processingQueueItemDatabaseOptions.TableName} set [State] = {(int)ProcessingQueueItemState.Skipped}
                        ,[SkippedTimestamp] = @SkippedTimestamp, [Message] = @message where [ProcessingQueueItemKey] = @ProcessingQueueItemKey";
            }
        }

        #endregion SQL
    }
}