using Autofac;
using Autofac.Extensions.DependencyInjection;
using Axerrio.BB.AspNetCore.Helpers.Converters;
using Axerrio.BB.AspNetCore.Helpers.Serialize;
using Axerrio.BB.DDD.Domain.Multitenancy;
using Axerrio.BB.DDD.Domain.Multitenancy.Extensions;
using Axerrio.BB.DDD.EntityFrameworkCore.Infrastructure;
using Axerrio.BB.DDD.EntityFrameworkCore.Infrastructure.ExecutionStrategy;
using Axerrio.BB.DDD.Extensions;
using Axerrio.BB.DDD.IE.AzureServiceBus.Infrastructure.Multitenancy;
using Axerrio.BB.DDD.Infrastructure.ExecutionStrategy.Abstractions;
using Axerrio.BB.DDD.Infrastructure.IntegrationEvents.Options;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.Cache;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.Middleware;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.Services;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.Services.Options;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.TenantResolvers;
using Axerrio.BB.DDD.Infrastructure.Multitenancy.TenantResolvers.Abstractions;
using Axerrio.BB.DDD.Sql.Extensions;
using Axerrio.BB.DDD.Sql.Infrastructure.Abstractions;
using Azure.Messaging.ServiceBus;
using Dapper;
using Floriday_Buyer_Sample.Infrastructure.AutofacModules;
using Floriday_Buyer_Sample.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using ProcessingQueue.Infrastructure;
using ProcessingQueue.Infrastructure.Options;
using Serilog;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("Settings/appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"Settings/appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Debug()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

#region configurations

builder.Configuration.AddConfiguration(configuration);
builder.Services.Configure<TenantResolutionMiddlewareOptions>(builder.Configuration.GetSection("TenantResolutionMiddlewareOptions"));
builder.Services.Configure<TenantUserResolutionMiddlewareOptions>(builder.Configuration.GetSection("TenantUserResolutionMiddlewareOptions"));
builder.Services.Configure<TenantCacheOptions>(builder.Configuration.GetSection("TenantCacheOptions"));
builder.Services.Configure<TenantUserCacheOptions>(builder.Configuration.GetSection("TenantUserCacheOptions"));
builder.Services.Configure<ProcessingQueueItemDatabaseOptions>(builder.Configuration.GetSection("ProcessingQueueItemDatabaseOptions"));
builder.Services.Configure<ClaimsPrincipalTenantUserResolverOptions>(builder.Configuration.GetSection("ClaimsPrincipalTenantUserResolverOptions"));
builder.Services.Configure<SingleDbConnectionOptions>(options => { options.ConnectionString = builder.Configuration["ConnectionString"]; });

#endregion configurations

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddTrustedMultitenancyHttpServices<TrustedTenant, TrustedTenantUser>(builder.Configuration);
builder.Services.TryAddSingleton<IIdentityConverter<int>>(IdentityConverter.DefaultConverters.ToIntConverter);

builder.Services.AddMultitenancyCoreServices<TrustedTenant, JsonSerializer, TrustedTenantService, TenantServiceOptions, TrustedTenantUser, TrustedTenantUserService<TrustedTenant>, TenantUserServiceOptions>(builder.Configuration, AxerrioTenantType.ABSTenantType);
//builder.Services.TryAddTransient<ITenantUserResolver<TrustedTenant, TrustedTenantUser, HttpContext>, HttpContextClaimsPrincipalTenantUserResolver<TrustedTenant, TrustedTenantUser>>();
//builder.Services.TryAddTransient<ITenantResolver<TrustedTenant, ServiceBusMessage>, HttpContextClaimsPrincipalTenantUserResolver<TrustedTenant, TrustedTenantUser>>();
//builder.Services.TryAddTransient<ITenantUserResolver<TrustedTenant, TrustedTenantUser, HttpContext>, HttpContextClaimsPrincipalTenantUserResolver<TrustedTenant, TrustedTenantUser>>();

builder.Services.TryAddTransient<ITenantResolver<TrustedTenant, ServiceBusMessage>, AzureServiceBusMessageTenantResolver<TrustedTenant>>();
builder.Services.TryAddTransient<ITenantUserResolver<TrustedTenant, TrustedTenantUser, ServiceBusMessage>, AzureServiceBusMessageTenantUserResolver<TrustedTenant, TrustedTenantUser>>();

builder.Services.AddMultitenancyHttpServices<TrustedTenant, HttpContextHeaderTenantResolver<TrustedTenant>, HttpContextHeaderTenantResolverOptions, TrustedTenantUser, HttpContextHeaderTenantUserResolver<TrustedTenant, TrustedTenantUser>, HttpContextHeaderTenantUserResolverOptions>(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region sql

builder.Services.AddEntityFrameworkSqlServer()
    .AddSimpleDbContext<ProcessingQueueItemDbContext>(builder.Configuration["ConnectionString"])
    .AddSimpleDbContext<ClientRequestDbContext>(builder.Configuration["ConnectionString"]);

builder.Services.AddTransient<IDbExecutionStrategyFactory<ProcessingQueueItemDbContext>, DbContextExecutionStrategyFactory<ProcessingQueueItemDbContext>>();

builder.Services.AddTransient<IDbQueryService<SqlMapper.GridReader>, DapperDbQueryService>();
builder.Services.AddTransient<IDbQueryService, DapperDbQueryService>();
builder.Services.AddTransient<IDdrDbConnectionFactory<int>, SingleDbConnectionFactory<int>>();
builder.Services.AddSqlExecutionStrategyServices().AddStrategyBuilder<ProcessingQueueItemProcessing<TrustedTenant, TrustedTenantUser>>(builder => builder.RetryAsync());

#endregion sql

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(builder => builder.RegisterModule(new ApplicationModule()));

#region authentication

builder.Services.AddAuthentication("Bearer")
.AddJwtBearer("Bearer", options =>
{
    options.Authority = builder.Configuration["AxerrioAccounts:Authority"];
    options.Audience = builder.Configuration["AxerrioAccounts:Audience"];
    options.RequireHttpsMetadata = false;
});

#endregion authentication

var app = builder.Build();

app.MigrateDbContext<ProcessingQueueItemDbContext>()
   .MigrateDbContext<ClientRequestDbContext>();

app.UseSwagger();
app.UseSwaggerUI();

//app.UseMultitenancy<TrustedTenant, TrustedTenantUser>();
app.UseTrustedTenantUserResolutionMiddleware();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();