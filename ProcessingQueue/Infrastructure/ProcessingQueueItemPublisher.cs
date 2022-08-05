using Axerrio.BB.DDD.Domain.Multitenancy.Abstractions;
using EnsureThat;
using Microsoft.Extensions.Logging;
using ProcessingQueue.Domain.ProcessingQueueItems;
using ProcessingQueue.Infrastructure.Abstractions;

namespace ProcessingQueue.Infrastructure
{
    public class ProcessingQueueItemPublisher<TTenant, TTenantUser> : IProcessingQueueItemPublisher
        where TTenant : class, ITenant
        where TTenantUser : TenantUser, ITenantUser
    {
        private readonly ILogger<ProcessingQueueItemPublisher<TTenant, TTenantUser>> _logger;
        private readonly ITenantContextAccessor<TTenant, TTenantUser> _tenantContextAccessor;
        private readonly ProcessingQueueItemDbContext _context;

        public ProcessingQueueItemPublisher(ILogger<ProcessingQueueItemPublisher<TTenant, TTenantUser>> logger,
            ITenantContextAccessor<TTenant, TTenantUser> tenantContextAccessor, ProcessingQueueItemDbContext context)
        {
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
            _context = EnsureArg.IsNotNull(context, nameof(context));
            _tenantContextAccessor = EnsureArg.IsNotNull(tenantContextAccessor, nameof(tenantContextAccessor));
        }

        public Task PublishAsync<TQueueItem>(string instanceKey, TQueueItem queueItem, Guid queueItemId, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantContextAccessor.TenantContext.Tenant.TenantId;
            var tenantUserId = _tenantContextAccessor.TenantContext.TenantUser.UserId;

            var processingQueueItem = ProcessingQueueItem.Create(instanceKey, tenantId, tenantUserId, queueItem, queueItemId);

            return PublishAsync(processingQueueItem, cancellationToken);
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