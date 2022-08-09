using Axerrio.BB.DDD.Domain.Multitenancy.Abstractions;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.TenantResolvers.Abstractions;
using Azure.Messaging.ServiceBus;
using EnsureThat;
using Microsoft.Extensions.Logging;
using ProcessingQueue.Domain.ProcessingQueueItems;
using ProcessingQueue.Infrastructure.Abstractions;

namespace ProcessingQueue.Infrastructure
{
    public class ProcessingQueueItemIntegrationEventPublisher<TTenant, TTenantUser> : IProcessingQueueItemPublisher<ServiceBusMessage>
        where TTenant : class, ITenant
        where TTenantUser : TenantUser, ITenantUser
    {
        private readonly ILogger<ProcessingQueueItemIntegrationEventPublisher<TTenant, TTenantUser>> _logger;
        private readonly ITenantResolver<TTenant, ServiceBusMessage> _tenantResolver;
        private readonly ITenantUserResolver<TTenant, TTenantUser, ServiceBusMessage> _tenantUserResolver;
        private readonly ProcessingQueueItemDbContext _context;

        public ProcessingQueueItemIntegrationEventPublisher(ILogger<ProcessingQueueItemIntegrationEventPublisher<TTenant, TTenantUser>> logger,
            ITenantResolver<TTenant, ServiceBusMessage> tenantResolver,
            ITenantUserResolver<TTenant, TTenantUser, ServiceBusMessage> tenantUserResolver,
            ProcessingQueueItemDbContext context)
        {
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
            _context = EnsureArg.IsNotNull(context, nameof(context));
            _tenantResolver = EnsureArg.IsNotNull(tenantResolver, nameof(tenantResolver));
            _tenantUserResolver = EnsureArg.IsNotNull(tenantUserResolver, nameof(tenantUserResolver));
        }

        public async Task PublishAsync<TQueueItem>(ServiceBusMessage context, string entityName, string instanceKey, TQueueItem queueItem, Guid queueItemId, CancellationToken cancellationToken = default)
        {
            var tenant = await _tenantResolver.ResolveAsync(context, cancellationToken);

            var tenantUser = await _tenantUserResolver.ResolveAsync(tenant, context, cancellationToken);

            var processingQueueItem = ProcessingQueueItem.Create(entityName, instanceKey, tenant.TenantId, tenantUser.UserId, queueItem, queueItemId);

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