using Axerrio.BB.DDD.Domain.Multitenancy.Abstractions;
using Axerrio.BB.DDD.Infrastructure.ExecutionStrategy.Abstractions;
using Axerrio.BB.DDD.Infrastructure.Sharding.Options;
using Axerrio.BB.DDD.Sql.Infrastructure.Abstractions;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    }
}