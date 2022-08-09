using Axerrio.BB.DDD.Application.Commands;
using Axerrio.BB.DDD.Application.Commands.Abstractions;
using Axerrio.BB.DDD.Domain.Multitenancy.Abstractions;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.TenantResolvers.Abstractions;
using EnsureThat;
using Floriday_Buyer.WorkerService.Infrastructure.Extensions;
using Floriday_Buyer_Sample.Shared.Application.Commands;
using MediatR;
using ProcessingQueue.Domain.ProcessingQueueItems;
using ProcessingQueue.Infrastructure.Abstractions;
using System.Text.Json;

namespace Floriday_Buyer.WorkerService
{
    public class Worker<TTenant, TTenantUser> : BackgroundService
        where TTenant : class, ITenant
        where TTenantUser : TenantUser, ITenantUser
    {
        private readonly ILogger<Worker<TTenant, TTenantUser>> _logger;
        private readonly ITenantContextFactory<TTenant> _tenantContextFactory;
        private readonly ITenantContextFactory<TTenant, TTenantUser> _tenantUserContextFactory;
        private readonly ITenantUserResolver<TTenant, TTenantUser, ProcessingQueueItem> _tenantUserResolver;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private int consecutiveTimesSkipped = 0;

        public Worker(
            ILogger<Worker<TTenant, TTenantUser>> logger,
            IServiceScopeFactory serviceScopeFactory,
            ITenantContextFactory<TTenant> tenantContextFactory,
            ITenantContextFactory<TTenant, TTenantUser> tenantUserContextFactory,
            ITenantUserResolver<TTenant, TTenantUser, ProcessingQueueItem> tenantUserResolver)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _tenantContextFactory = EnsureArg.IsNotNull(tenantContextFactory, nameof(tenantContextFactory));
            _tenantUserContextFactory = EnsureArg.IsNotNull(tenantUserContextFactory, nameof(tenantUserContextFactory));
            _tenantUserResolver = EnsureArg.IsNotNull(tenantUserResolver, nameof(tenantUserResolver));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                // deze hardcoded lijst, moet vervangen worden door een Get om alle actieve tenantids op te halen
                var tenantIds = new List<int> { 1, 2, 3, 4, 5 };
                var workerTasks = new List<Task<bool>>();
                foreach (var tenantId in tenantIds)
                {
                    workerTasks.Add(HandleEventsForTenantAsync(tenantId, stoppingToken));
                }

                var results = await Task.WhenAll(workerTasks);

                if (results.All(r => r.Equals(false)))
                    consecutiveTimesSkipped++;
                else
                    consecutiveTimesSkipped = 0;

                await Task.Delay(TimeSpan.FromSeconds(consecutiveTimesSkipped), stoppingToken);
            }
        }

        public async Task<bool> HandleEventsForTenantAsync(int tenantId, CancellationToken stoppingToken)
        {
            var scope = _serviceScopeFactory.CreateScope();

            var tenant = Activator.CreateInstance(typeof(TTenant), tenantId) as TTenant;
            var tenantContext = _tenantContextFactory.Create(tenant);

            var consumerService = scope.ServiceProvider.GetRequiredService<IProcessingQueueItemConsumer>();
            var itemsToProcess = await consumerService.GetEventsToProcessAsync(stoppingToken);

            _logger.LogInformation($"Worker - Found {itemsToProcess.Count()} events to process for tenant: {tenantId}.");

            foreach (var item in itemsToProcess)
            {
                _logger.LogInformation($"Worker - Resolving tenantUser for tenant: {tenantId}.");

                var tenantUser = await _tenantUserResolver.ResolveAsync(tenant, item, stoppingToken);
                var tenantUserContext = _tenantUserContextFactory.Create(tenant, tenantUser);

                _logger.LogInformation($"Worker - Resolved tenantUser for tenant: {tenantId}.");

                await HandleProcessingEventAsync(scope, item, stoppingToken);

                _tenantUserContextFactory.Dispose(tenantUserContext);
            }

            _tenantContextFactory.Dispose(tenantContext);
            return itemsToProcess.Any();
        }

        public async Task HandleProcessingEventAsync(IServiceScope scope, ProcessingQueueItem item, CancellationToken stoppingToken)
        {
            var consumerService = scope.ServiceProvider.GetRequiredService<IProcessingQueueItemConsumer>();

            try
            {
                var result = await RunEventAsCommandAsync(scope, item, stoppingToken);

                if (result.Success)
                    await consumerService.MarkEventProcessedAsync(item, stoppingToken);
                else
                    await consumerService.MarkEventFailedAsync(item, item.CreateErrorMessage(result), stoppingToken);
            }
            catch (Exception exc)
            {
                await consumerService.MarkEventFailedAsync(item, item.CreateErrorMessage(exc), stoppingToken);

                _logger.LogError(exc, $"An error occurred when processing event: {item.ProcessingQueueItemKey}");
            }
        }

        public async Task<CommandResult> RunEventAsCommandAsync(IServiceScope scope, ProcessingQueueItem item, CancellationToken stoppingToken)
        {
            CommandResult result = item.EventName switch
            {
                nameof(CreateTestCommand) => await GetCommandResultAsync<CreateTestCommand>(scope, item, stoppingToken),
                _ => new CommandResult.Failed(_logger, Guid.NewGuid()),
            };

            return result;
        }

        public async Task<CommandResult> GetCommandResultAsync<TCommand>(IServiceScope scope, ProcessingQueueItem item, CancellationToken stoppingToken)
            where TCommand : Command<TCommand>
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var command = JsonSerializer.Deserialize<TCommand>(item.EventContent);

            if (command == null) return new CommandResult.Failed(_logger, requestId: null);

            return await mediator.ExecuteCommandAsync(command, stoppingToken);
        }
    }
}