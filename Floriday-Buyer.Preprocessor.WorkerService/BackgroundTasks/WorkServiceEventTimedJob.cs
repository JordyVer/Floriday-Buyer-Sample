using Axerrio.BB.DDD.Domain.Multitenancy;
using Axerrio.BB.DDD.Domain.Multitenancy.Abstractions;
using Axerrio.BB.DDD.Job.BackgroundTasks.TimedJobs.Abstractions;
using Axerrio.BB.DDD.Job.Domain.Aggregates.JobAggregate;
using EnsureThat;
using ProcessingQueue.Infrastructure.Abstractions;
using System.Diagnostics;

namespace Floriday_Buyer.Preprocessor.WorkerService.BackgroundTasks
{
    public class WorkServiceEventTimedJob : ManagedJob
    {
        private readonly ITenantContextFactory<TrustedTenant> _tenantContextFactory;
        private readonly ITenantContextFactory<TrustedTenant, TrustedTenantUser> _tenantUserContextFactory;

        private TrustedTenantUser _tenantUser;
        private TenantContext<TrustedTenant> _tenantContext;
        private TenantContext<TrustedTenant, TrustedTenantUser> _tenantUserContext;

        public WorkServiceEventTimedJob(ILogger<WorkServiceEventTimedJob> logger, IServiceProvider provider, int jobSettingId,
            ITenantContextFactory<TrustedTenant> tenantContextFactory,
            ITenantContextFactory<TrustedTenant, TrustedTenantUser> tenantUserContextFactory)
            : base(logger, provider, jobSettingId, "WorkServiceEventTimedJob")
        {
            _tenantContextFactory = EnsureArg.IsNotNull(tenantContextFactory, nameof(tenantContextFactory));
            _tenantUserContextFactory = EnsureArg.IsNotNull(tenantUserContextFactory, nameof(tenantUserContextFactory));
        }

        public override async Task DoWorkAsync(IServiceScope scope, CancellationToken cancellationToken = default)
        {
            try
            {
                var processingService = scope.ServiceProvider.GetRequiredService<IProcessingQueueItemProcessing>();
                var itemsForProcessing = await processingService.GetEventsForPreprocessingAsync(cancellationToken);

                foreach (var item in itemsForProcessing)
                {
                    try
                    {
                        SetupTenantAnsSystemUserContext(int.Parse(item.TenantId), int.Parse(item.TenantId));
                        // TODO implment real pre processing here!!!
                        bool success = (new Random().Next(10) > 5);

                        if (success)
                            await processingService.MarkEventReadyToProcessAsync(item, cancellationToken);
                        else
                            await processingService.MarkEventSkippedAsync(item, cancellationToken);
                    }
                    catch (Exception exc)
                    {
                    }
                    finally
                    {
                        DisposeTenantAndTenantUserContext();
                    }
                }

                Job.UpdateStatus(JobStatus.Success, $"Successfully Preprocessed {itemsForProcessing.Count()} processing queuitems");
            }
            catch (Exception exc)
            {
                Job.UpdateStatus(JobStatus.Failed, $"Failed to Preprocessed queue items {exc.Demystify()}");
            }
        }

        private void SetupTenantAnsSystemUserContext(int tenantid, int tenantUserId)
        {
            var tenant = new TrustedTenant(tenantid);
            _logger.LogDebug($"Creating tenantContextFactory for tenantId {tenant.TenantId}");
            _tenantContext = _tenantContextFactory.Create(tenant);
            _tenantUser = new TrustedTenantUser(tenantUserId);
            _logger.LogDebug($"Creating tenantUserContextFactory for tenantUserId {_tenantUser.UserId}");
            _tenantUserContext = _tenantUserContextFactory.Create(tenant, _tenantUser);
        }

        private void DisposeTenantAndTenantUserContext()
        {
            if (_tenantContext != null) _tenantContextFactory.Dispose(_tenantContext);
            if (_tenantUserContext != null) _tenantContextFactory.Dispose(_tenantUserContext);
        }
    }
}