using Autofac;
using Autofac.Extensions.DependencyInjection;
using Axerrio.BB.AspNetCore.HealthChecks.Extensions;
using Axerrio.BB.AspNetCore.Helpers.Converters;
using Axerrio.BB.AspNetCore.Helpers.Serialize;
using Axerrio.BB.DDD.Domain.Multitenancy;
using Axerrio.BB.DDD.Domain.Multitenancy.Extensions;
using Axerrio.BB.DDD.Infrastructure.IntegrationEvents.Options;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.Services;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.Services.Options;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.TenantResolvers.Abstractions;
using Axerrio.BB.DDD.Job.Infrastructure.HealthChecks.Extensions;
using Axerrio.BB.DDD.Sql.Extensions;
using Axerrio.BB.DDD.Sql.Infrastructure.Abstractions;
using Dapper;
using Floriday_Buyer.WorkerService;
using Floriday_Buyer.WorkerService.Infrastructure.AutofacModules;
using Floriday_Buyer_Sample.Shared.Application.Commands;
using Floriday_Buyer_Sample.Shared.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using ProcessingQueue.Domain.ProcessingQueueItems;
using ProcessingQueue.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Debug()
    .CreateLogger();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(hostConfig =>
    {
        hostConfig.SetBasePath(Directory.GetCurrentDirectory());
        hostConfig.AddJsonFile("Settings/appsettings.json", optional: true, reloadOnChange: true);
        hostConfig.AddJsonFile($"Settings/appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true);
    })
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureServices((hostContext, services) =>
    {
        services.AddMediatR(typeof(CreateTestCommand).Assembly);
        services.AddHostedService<Worker<TrustedTenant, TrustedTenantUser>>();
        services.Configure<SingleDbConnectionOptions>(options => { options.ConnectionString = hostContext.Configuration["ConnectionString"]; });
        services.AddServiceHealthChecks().AddQuartzHealthCheck();
        services.AddTransient<IDbQueryService<SqlMapper.GridReader>, DapperDbQueryService>();
        services.AddTransient<IDbQueryService, DapperDbQueryService>();
        services.AddTransient<IDdrDbConnectionFactory<int>, SingleDbConnectionFactory<int>>();
        services.TryAddSingleton<IIdentityConverter<int>>(IdentityConverter.DefaultConverters.ToIntConverter);

        services.TryAddTransient<ITenantUserResolver<TrustedTenant, TrustedTenantUser, ProcessingQueueItem>, ProcessingQueueTenantUserResolver<TrustedTenant, TrustedTenantUser>>();

        services.AddMultitenancyCoreServices<TrustedTenant, JsonSerializer, TrustedTenantService, TenantServiceOptions, TrustedTenantUser, TrustedTenantUserService<TrustedTenant>, TenantUserServiceOptions>(hostContext.Configuration, AxerrioTenantType.ABSTenantType);
        services.AddSqlExecutionStrategyServices().AddStrategyBuilder<ProcessingQueueItemConsumer<TrustedTenant, TrustedTenantUser>>(builder => builder.RetryAsync());
    })
    .ConfigureContainer<ContainerBuilder>(builder => builder.RegisterModule(new ApplicationModule()))
    .UseSerilog()
    .Build();

await host.RunAsync();