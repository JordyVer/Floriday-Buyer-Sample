using Autofac;
using Axerrio.BB.DDD.EntityFrameworkCore.Infrastructure.AutofacModules;
using ProcessingQueue.Domain.Services;
using ProcessingQueue.Domain.Services.Abstractions;

namespace Floriday_Buyer_Sample.Infrastructure.AutofacModules
{
    public class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterModule(new ClientRequestMediatorModule<Program>());

            builder.RegisterType<ProcessingQueueItemService>().As<IProcessingQueueItemService>().InstancePerLifetimeScope();
        }
    }
}