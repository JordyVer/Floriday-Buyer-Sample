using Axerrio.BB.DDD.Domain.IntegrationEvents.Abstractions;
using Axerrio.BB.DDD.Domain.Multitenancy.Abstractions;
using EnsureThat;
using Microsoft.Extensions.Logging;
using ProcessingQueue.Domain.ProcessingQueueItems;
using ProcessingQueue.Infrastructure.Abstractions;

namespace ProcessingQueue.Infrastructure
{
    public class ProcessingQueueItemIntegrationEventPublisher<TTenant, TTenantUser> : IProcessingQueueItemPublisher<IntegrationEvent>
        where TTenant : class, ITenant
        where TTenantUser : TenantUser, ITenantUser
    {
        private readonly ILogger<ProcessingQueueItemIntegrationEventPublisher<TTenant, TTenantUser>> _logger;
        private readonly ITenantContextAccessor<TTenant> _tenantContextAccessor;
        private readonly ITenantContextAccessor<TTenant, TTenantUser> _tenantUserContextAccessor;
        private readonly ProcessingQueueItemDbContext _context;

        public ProcessingQueueItemIntegrationEventPublisher(ILogger<ProcessingQueueItemIntegrationEventPublisher<TTenant, TTenantUser>> logger,
            ITenantContextAccessor<TTenant> tenantContextAccessor,
            ITenantContextAccessor<TTenant, TTenantUser> tenantUserContextAccessor,
            ProcessingQueueItemDbContext context)
        {
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
            _context = EnsureArg.IsNotNull(context, nameof(context));
            _tenantContextAccessor = EnsureArg.IsNotNull(tenantContextAccessor, nameof(tenantContextAccessor));
            _tenantUserContextAccessor = EnsureArg.IsNotNull(tenantUserContextAccessor, nameof(tenantUserContextAccessor));
        }

        public async Task PublishAsync<TQueueItem>(IntegrationEvent context, string entityName, string instanceKey, TQueueItem queueItem, Guid queueItemId, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantContextAccessor.TenantContext.Tenant.TenantId;

            var tenantUserId = _tenantUserContextAccessor.TenantContext.TenantUser.UserId;

            var processingQueueItem = ProcessingQueueItem.Create(entityName, instanceKey, tenantId, tenantUserId, queueItem, queueItemId);

            await PublishAsync(processingQueueItem, cancellationToken);
        }

        private async Task PublishAsync(ProcessingQueueItem processingQueueItem, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug($"Enqueueing event: {processingQueueItem.EventName}");

            await _context.ProcessingQueueItems.AddAsync(processingQueueItem, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug($"Enqueued queue item: {processingQueueItem.ProcessingQueueItemKey} event: {processingQueueItem.EventName}");
        }
    }
}