using Autofac;
using Autofac.Extensions.DependencyInjection;
using Floriday_Buyer.Preprocessor.WorkerService;
using Floriday_Buyer.Preprocessor.WorkerService.Infrastructure.AutofacModules;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args)
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .ConfigureContainer<ContainerBuilder>(builder => builder.RegisterModule(new ApplicationModule()))
    .UseSerilog()
    .Build();

await host.RunAsync();