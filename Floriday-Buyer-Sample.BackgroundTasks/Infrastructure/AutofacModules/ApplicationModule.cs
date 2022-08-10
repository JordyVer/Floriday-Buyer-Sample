using Autofac;
using Axerrio.BB.DDD.Job.Infrastructure.AutofacModules;
using Axerrio.BB.DDD.Job.Infrastructure.Extensions;
using Floriday_Buyer.Shared.BackgroundTasks;
using Floriday_Buyer_Sample.BackgroundTasks.BackgroundTasks;
using ProcessingQueue.Infrastructure;
using ProcessingQueue.Infrastructure.Abstractions;

namespace Floriday_Buyer_Sample.BackgroundTasks.Infrastructure.AutofacModules
{
    public class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterModule(new JobModule<BuyerEventHostedService>());

            builder.RegisterJob<CleanupEventsEventTimedJob>((int)JobIdentifiers.CleanupEvents);
            builder.RegisterJob<NotifyFailedEventTimedJob>((int)JobIdentifiers.NotifyFailed);

            builder.RegisterType<ProcessingQueueItemCleanup>()
                .As<IProcessingQueueItemCleanup>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ProcessingQueueItemNotify>()
                .As<IProcessingQueueItemNotify>()
                .InstancePerLifetimeScope();
        }
    }
}