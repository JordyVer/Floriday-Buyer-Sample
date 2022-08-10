using Autofac;
using Autofac.Extensions.DependencyInjection;
using Axerrio.BB.AspNetCore.EntityFrameworkCore.Extensions.Hosting;
using Axerrio.BB.AspNetCore.Helpers.Serialize;
using Axerrio.BB.DDD.Domain.Multitenancy;
using Axerrio.BB.DDD.Domain.Multitenancy.Extensions;
using Axerrio.BB.DDD.Infrastructure.IntegrationEvents.Options;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.Services;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.Services.Options;
using Axerrio.BB.DDD.Job.Infrastructure;
using Axerrio.BB.DDD.Sql.Extensions;
using Axerrio.BB.DDD.Sql.Infrastructure.Abstractions;
using Dapper;
using Floriday_Buyer_Sample.BackgroundTasks.Infrastructure.AutofacModules;
using Floriday_Buyer_Sample.BackgroundTasks.Infrastructure.Extensions;
using Floriday_Buyer_Sample.Infrastructure.Extensions;
using Polly;
using ProcessingQueue.Infrastructure;
using ProcessingQueue.Infrastructure.Options;
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
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<SingleDbConnectionOptions>(options => { options.ConnectionString = hostContext.Configuration["ConnectionString"]; });
        services.Configure<ProcessingQueueItemCleanupOptions>(opt => new ProcessingQueueItemCleanupOptions());
        services.AddSimpleDbContext<JobDbContext>(hostContext.Configuration["ConnectionString"]);
        services.AddTransient<IDbQueryService<SqlMapper.GridReader>, DapperDbQueryService>();
        services.AddTransient<IDbQueryService, DapperDbQueryService>();
        services.AddTransient<IDdrDbConnectionFactory<int>, SingleDbConnectionFactory<int>>();
        services.AddMultitenancyCoreServices<TrustedTenant, JsonSerializer, TrustedTenantService, TenantServiceOptions, TrustedTenantUser, TrustedTenantUserService<TrustedTenant>, TenantUserServiceOptions>(hostContext.Configuration, AxerrioTenantType.ABSTenantType);
        services.AddSqlExecutionStrategyServices()
                .AddStrategyBuilder<ProcessingQueueItemCleanup>(builder => builder.RetryAsync())
                .AddStrategyBuilder<ProcessingQueueItemNotify>(builder => builder.RetryAsync());
    })
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(builder => builder.RegisterModule(new ApplicationModule()))
    .UseSerilog()
    .Build();

host.MigrateDbContext<JobDbContext>((context, provider) => JobSettingSeeder.Seed(context));

await host.RunAsync();