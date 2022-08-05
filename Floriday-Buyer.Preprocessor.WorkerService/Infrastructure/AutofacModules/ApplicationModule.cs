using Autofac;
using Axerrio.BB.DDD.Job.BackgroundTasks.TimedJobs;
using Axerrio.BB.DDD.Job.Infrastructure.Extensions;
using Floriday_Buyer.Preprocessor.WorkerService.BackgroundTasks;

namespace Floriday_Buyer.Preprocessor.WorkerService.Infrastructure.AutofacModules
{
    internal class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            //builder.RegisterModule(new JobModule<PreprocessingEventHostedService>());

            builder.RegisterJob<WorkServiceEventTimedJob>((int)JobIdentifiers.WorkService);
            builder.RegisterJob<JobHistoryCleanupTimedJob>((int)JobIdentifiers.JobHistoryCleanup);
        }
    }
}