using Axerrio.BB.AspNetCore.Helpers.Converters;
using Axerrio.BB.DDD.Domain.Multitenancy.Abstractions;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.Services.Abstractions;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.TenantResolvers.Abstractions;
using EnsureThat;
using Microsoft.Extensions.Logging;
using ProcessingQueue.Domain.ProcessingQueueItems;

namespace Floriday_Buyer_Sample.Shared.Infrastructure
{
    public class ProcessingQueueTenantUserResolver<TTenant, TTenantUser> : TenantUserResolver<TTenant, TTenantUser, ProcessingQueueItem>
        where TTenant : class, ITenant
        where TTenantUser : TenantUser, ITenantUser
    {
        private readonly IIdentityConverter<int> _tenantUserIdConverter;
        private readonly ILogger<ProcessingQueueTenantUserResolver<TTenant, TTenantUser>> _logger;

        public ProcessingQueueTenantUserResolver(ITenantUserService<TTenant, TTenantUser> tenantUserService
             , IIdentityConverter<int> tenantIdConverter
            , ILogger<ProcessingQueueTenantUserResolver<TTenant, TTenantUser>> logger) : base(tenantUserService)
        {
            _tenantUserIdConverter = EnsureArg.IsNotNull(tenantIdConverter, nameof(tenantIdConverter));
            _logger = logger;
        }

        protected override async Task<TTenantUser> ResolveAsync(TTenant tenant, ProcessingQueueItem context, CancellationToken cancellationToken = default)
        {
            var tenantUserIdAsString = context.TenantUserId;
            if (!_tenantUserIdConverter.TryConvert(tenantUserIdAsString, out var userId))
            {
                _logger.LogInformation($"Could not convert {tenantUserIdAsString} to userId.");
                return null;
            }

            return await _tenantUserService.GetTenantUserAsync(userId, cancellationToken);
        }
    }
}