using Autofac;
using Axerrio.BB.DDD.Domain.Multitenancy;
using Axerrio.BB.DDD.EntityFrameworkCore.Infrastructure.AutofacModules;
using ProcessingQueue.Infrastructure;
using ProcessingQueue.Infrastructure.Abstractions;

namespace Floriday_Buyer_Sample.Infrastructure.AutofacModules
{
    public class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterModule(new ClientRequestMediatorModule<Program>());

            builder.RegisterType<ProcessingQueueItemPublisher<TrustedTenant, TrustedTenantUser>>().As<IProcessingQueueItemPublisher>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessingQueueItemProcessing<TrustedTenant, TrustedTenantUser>>().As<IProcessingQueueItemProcessing>().InstancePerLifetimeScope();
        }
    }
}