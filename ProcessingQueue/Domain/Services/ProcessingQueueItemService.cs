using Axerrio.BB.DDD.Domain.Multitenancy;
using Axerrio.BB.DDD.Domain.Multitenancy.Abstractions;
using ProcessingQueue.Domain.Services.Abstractions;

namespace ProcessingQueue.Domain.Services
{
    public class ProcessingQueueItemService : IProcessingQueueItemService
    {
        private readonly ITenantContextAccessor<TrustedTenant, TrustedTenantUser> _tenantContextAccessor;

        public ProcessingQueueItemService(ITenantContextAccessor<TrustedTenant, TrustedTenantUser> tenantContextAccessor)
        {
            _tenantContextAccessor = tenantContextAccessor;
        }

        public Task<int> AddAsync<TQueueItem>(string instanceKey, TQueueItem queueItem, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantContextAccessor.TenantContext.Tenant.TenantId;
            var userId = _tenantContextAccessor.TenantContext.TenantUser.UserId;
            //var processingQueueItem = ProcessingQueueItem.Create(tenantId, userId, queueItem, instanceKey, default);
            // open connection and store

            return Task.FromResult(1);
        }
    }
}