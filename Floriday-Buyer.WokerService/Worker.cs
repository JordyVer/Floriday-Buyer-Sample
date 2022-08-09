using Axerrio.BB.DDD.Application.Commands;
using Axerrio.BB.DDD.Application.Commands.Abstractions;
using Axerrio.BB.DDD.Domain.Multitenancy;
using Axerrio.BB.DDD.Domain.Multitenancy.Abstractions;
using EnsureThat;
using Floriday_Buyer_Sample.Shared.Application.Commands;
using MediatR;
using ProcessingQueue.Domain.ProcessingQueueItems;
using ProcessingQueue.Infrastructure.Abstractions;
using System.Text.Json;

namespace Floriday_Buyer.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IMediator _mediator;
        private readonly ITenantContextFactory<TrustedTenant> _tenantContextFactory;
        private readonly ITenantContextFactory<TrustedTenant, TrustedTenantUser> _tenantUserContextFactory;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private TrustedTenantUser _tenantUser;
        private TenantContext<TrustedTenant> _tenantContext;
        private TenantContext<TrustedTenant, TrustedTenantUser> _tenantUserContext;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory,
                ITenantContextFactory<TrustedTenant> tenantContextFactory,
                ITenantContextFactory<TrustedTenant, TrustedTenantUser> tenantUserContextFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _tenantContextFactory = EnsureArg.IsNotNull(tenantContextFactory, nameof(tenantContextFactory));
            _tenantUserContextFactory = EnsureArg.IsNotNull(tenantUserContextFactory, nameof(tenantUserContextFactory));
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                try
                {
                    var scope = _serviceScopeFactory.CreateScope();
                    SetupTenantAnsSystemUserContext(); //TODO resolver maken voor queue
                    _mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    var consumerService = scope.ServiceProvider.GetRequiredService<IProcessingQueueItemConsumer>();

                    var itemsToProcess = await consumerService.GetEventsToProcessAsync(stoppingToken);
                    foreach (var item in itemsToProcess)
                    {
                        CommandResult result = item.EventName switch
                        {
                            nameof(CreateTestCommand) => await GetCommandResultAsync<CreateTestCommand>(item),
                            _ => new CommandResult.Failed(_logger, Guid.NewGuid()),
                        };
                        if (result.Success)
                            await consumerService.MarkEventProcessedAsync(item, stoppingToken);
                        else
                            await consumerService.MarkEventFailedAsync(item, stoppingToken);
                    }
                }
                catch (Exception exc)
                {
                    _logger.LogError(exc, "");
                }
                finally
                {
                    DisposeTenantAndTenantUserContext();
                }

                await Task.Delay(1000, stoppingToken);
            }
        }

        public async Task<CommandResult> GetCommandResultAsync<TCommand>(ProcessingQueueItem item) where TCommand : Command<TCommand>
        {
            var command = JsonSerializer.Deserialize<TCommand>(item.EventContent);

            if (command == null) return new CommandResult.Failed(_logger, Guid.NewGuid());

            return await _mediator.ExecuteCommandAsync(command);
        }

        private void SetupTenantAnsSystemUserContext()
        {
            var tenant = new TrustedTenant(1);
            _logger.LogDebug($"Creating tenantContextFactory for tenantId {tenant.TenantId}");
            _tenantContext = _tenantContextFactory.Create(tenant);
            _tenantUser = new TrustedTenantUser(TenantUser.SystemUserId);
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