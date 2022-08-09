using Axerrio.BB.DDD.Domain.Multitenancy.Abstractions;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.TenantResolvers.Abstractions;
using EnsureThat;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProcessingQueue.Domain.ProcessingQueueItems;
using ProcessingQueue.Infrastructure.Abstractions;

namespace ProcessingQueue.Infrastructure
{
    public class ProcessingQueueItemHttpPublisher<TTenant, TTenantUser> : IProcessingQueueItemPublisher<HttpContext>
        where TTenant : class, ITenant
        where TTenantUser : TenantUser, ITenantUser
    {
        private readonly ILogger<ProcessingQueueItemHttpPublisher<TTenant, TTenantUser>> _logger;
        private readonly ITenantContextAccessor<TTenant, TTenantUser> _tenantContextAccessor;
        private readonly ITenantUserResolver<TTenant, TTenantUser, HttpContext> _tenantUserResolver;
        private readonly ProcessingQueueItemDbContext _context;

        public ProcessingQueueItemHttpPublisher(ILogger<ProcessingQueueItemHttpPublisher<TTenant, TTenantUser>> logger,
            ITenantContextAccessor<TTenant, TTenantUser> tenantContextAccessor,
            ITenantUserResolver<TTenant, TTenantUser, HttpContext> tenantUserResolver,
            ProcessingQueueItemDbContext context)
        {
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
            _context = EnsureArg.IsNotNull(context, nameof(context));
            _tenantContextAccessor = EnsureArg.IsNotNull(tenantContextAccessor, nameof(tenantContextAccessor));
            _tenantUserResolver = EnsureArg.IsNotNull(tenantUserResolver, nameof(tenantUserResolver));
        }

        public async Task PublishAsync<TQueueItem>(HttpContext context, string entityName, string instanceKey, TQueueItem queueItem, Guid queueItemId, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantContextAccessor.TenantContext.Tenant.TenantId;
            var tenant = Activator.CreateInstance(typeof(TTenant), tenantId) as TTenant;

            var tenantUser = await _tenantUserResolver.ResolveAsync(tenant, context, cancellationToken);

            var processingQueueItem = ProcessingQueueItem.Create(entityName, instanceKey, tenantId, tenantUser.UserId, queueItem, queueItemId);

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