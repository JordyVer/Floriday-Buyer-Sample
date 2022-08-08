using Autofac;
using Axerrio.BB.DDD.Domain.Multitenancy;
using Axerrio.BB.DDD.Job.Infrastructure.AutofacModules;
using Axerrio.BB.DDD.Job.Infrastructure.Extensions;
using Floriday_Buyer.Preprocessor.WorkerService.BackgroundTasks;
using Floriday_Buyer.Shared.BackgroundTasks;
using ProcessingQueue.Infrastructure;
using ProcessingQueue.Infrastructure.Abstractions;

namespace Floriday_Buyer.Preprocessor.WorkerService.Infrastructure.AutofacModules
{
    internal class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterModule(new JobModule<PreprocessingEventHostedService>());

            builder.RegisterJob<WorkServiceEventTimedJob>((int)JobIdentifiers.WorkService);

            builder.RegisterType<ProcessingQueueItemProcessing<TrustedTenant, TrustedTenantUser>>()
                .As<IProcessingQueueItemProcessing>()
                .InstancePerLifetimeScope();
        }
    }
}