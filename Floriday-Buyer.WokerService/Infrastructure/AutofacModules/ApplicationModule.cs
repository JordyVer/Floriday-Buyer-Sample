using Autofac;
using Axerrio.BB.DDD.Domain.Multitenancy;
using ProcessingQueue.Infrastructure;
using ProcessingQueue.Infrastructure.Abstractions;

namespace Floriday_Buyer.WorkerService.Infrastructure.AutofacModules
{
    public class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            //builder.RegisterModule<MediatorModule<CreateTestCommand>>();

            //builder.RegisterType<CreateTestCommandValidator>()
            //   .AsImplementedInterfaces();

            //builder.RegisterModule(new JobModule<ProcessingEventHostedService>());

            //builder.RegisterJob<ConsumerWorkerServiceEventTimedJob>((int)JobIdentifiers.ConsumerWorkerService);

            builder.RegisterType<ProcessingQueueItemConsumer<TrustedTenant, TrustedTenantUser>>()
                .As<IProcessingQueueItemConsumer>()
                .InstancePerLifetimeScope();
        }
    }
}