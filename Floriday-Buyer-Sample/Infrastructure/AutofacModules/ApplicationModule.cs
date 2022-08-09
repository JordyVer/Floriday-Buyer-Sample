using Autofac;
using Axerrio.BB.DDD.Domain.Multitenancy;
using Axerrio.BB.DDD.EntityFrameworkCore.Infrastructure.AutofacModules;
using Azure.Messaging.ServiceBus;
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

            builder.RegisterType<ProcessingQueueItemHttpPublisher<TrustedTenant, TrustedTenantUser>>().As<IProcessingQueueItemPublisher<HttpContext>>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessingQueueItemIntegrationEventPublisher<TrustedTenant, TrustedTenantUser>>().As<IProcessingQueueItemPublisher<ServiceBusMessage>>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessingQueueItemProcessing<TrustedTenant, TrustedTenantUser>>().As<IProcessingQueueItemProcessing>().InstancePerLifetimeScope();
        }
    }
}