using Axerrio.BB.DDD.Job.Domain.Aggregates.JobSettingAggregate;
using Axerrio.BB.DDD.Job.Infrastructure;
using Floriday_Buyer.Shared.BackgroundTasks;

namespace Floriday_Buyer.Preprocessor.WorkerService.Infrastructure.Extensions
{
    public static class JobSettingSeeder
    {
        public static bool Seed(JobDbContext context)
        {
            var currentJobSettings = context.JobSettings.ToList();
            var seedJobSettings = new List<JobSetting>
            {
               JobSetting.Create((int)JobIdentifiers.WorkService, "WorkServiceEventTimedJob", "0 0/1 * 1/1 * ? *", true),
            };

            var jobSettings = seedJobSettings
               .Where(sjs => !currentJobSettings.Any(cjs => cjs.JobSettingID == sjs.JobSettingID));

            if (jobSettings.Any())
            {
                context.AddRange(jobSettings);
                context.SaveChanges();

                return true;
            }

            return false;
        }
    }
}