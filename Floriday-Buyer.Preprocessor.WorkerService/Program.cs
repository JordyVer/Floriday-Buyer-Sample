using Autofac;
using Autofac.Extensions.DependencyInjection;
using Axerrio.BB.AspNetCore.HealthChecks.Extensions;
using Axerrio.BB.AspNetCore.Helpers.Serialize;
using Axerrio.BB.DDD.Domain.Multitenancy;
using Axerrio.BB.DDD.Domain.Multitenancy.Extensions;
using Axerrio.BB.DDD.Infrastructure.IntegrationEvents.Options;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.Services;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.Services.Options;
using Axerrio.BB.DDD.Job.Infrastructure;
using Axerrio.BB.DDD.Job.Infrastructure.HealthChecks.Extensions;
using Axerrio.BB.DDD.Sql.Extensions;
using Axerrio.BB.DDD.Sql.Infrastructure.Abstractions;
using Dapper;
using Floriday_Buyer.Preprocessor.WorkerService.Infrastructure.AutofacModules;
using Floriday_Buyer.Preprocessor.WorkerService.Infrastructure.Extensions;
using Floriday_Buyer_Sample.Infrastructure.Extensions;
using Floriday_Buyer_Sample.Shared.Extensions;
using Polly;
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
        services.Configure<SingleDbConnectionOptions>(options => { options.ConnectionString = hostContext.Configuration["ConnectionString"]; });
        services.AddSimpleDbContext<JobDbContext>(hostContext.Configuration["ConnectionString"]);
        services.AddServiceHealthChecks().AddQuartzHealthCheck();
        services.AddTransient<IDbQueryService<SqlMapper.GridReader>, DapperDbQueryService>();
        services.AddTransient<IDbQueryService, DapperDbQueryService>();
        services.AddTransient<IDdrDbConnectionFactory<int>, SingleDbConnectionFactory<int>>();
        services.AddMultitenancyCoreServices<TrustedTenant, JsonSerializer, TrustedTenantService, TenantServiceOptions, TrustedTenantUser, TrustedTenantUserService<TrustedTenant>, TenantUserServiceOptions>(hostContext.Configuration, AxerrioTenantType.ABSTenantType);
        services.AddSqlExecutionStrategyServices().AddStrategyBuilder<ProcessingQueueItemProcessing<TrustedTenant, TrustedTenantUser>>(builder => builder.RetryAsync());
    })
    .ConfigureContainer<ContainerBuilder>(builder => builder.RegisterModule(new ApplicationModule()))
    .UseSerilog()
    .Build();

host.MigrateDbContext<JobDbContext>((context, provider) => JobSettingSeeder.Seed(context));

await host.RunAsync();