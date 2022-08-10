using Axerrio.BB.AspNetCore.Quartz.Infrastructure.Hosting.HostedServices;
using Axerrio.BB.AspNetCore.Quartz.Infrastructure.SchedulerListener;
using Axerrio.BB.AspNetCore.Quartz.Model;
using Axerrio.BB.DDD.Job.Domain.Aggregates.JobSettingAggregate.Abstractions;
using EnsureThat;
using Quartz.Spi;
using System.Diagnostics;

namespace Floriday_Buyer_Sample.BackgroundTasks.BackgroundTasks
{
    internal class BuyerEventHostedService : HostedService
    {
        private readonly ILogger<HostedService> _logger;

        public BuyerEventHostedService(ILogger<HostedService> logger, IJobFactory jobFactory, IJobSettingRepository jobSettingRepository, QuartzSchedulerListener listener) : base(logger, jobFactory, listener)
        {
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));

            var jobSettings = jobSettingRepository.GetAllActiveAsync().Result;

            if (jobSettings.Count() == 0)
            {
                logger.LogError($"{nameof(BuyerEventHostedService)} - Couldn't retrieve settings from the database");
                return;
            }

            try
            {
                foreach (var jobSetting in jobSettings)
                {
                    if (jobSetting == null)
                    {
                        logger.LogError($"{nameof(BuyerEventHostedService)} - Couldn't register job: {jobSetting.Name} with ID: {jobSetting.JobSettingID}, because job settings were not found!");
                        continue;
                    }

                    AddJobsWithHostedServiceJobName(jobSetting.Name, jobSetting);
                }
            }
            catch (Exception exc)
            {
                logger.LogError($"{nameof(BuyerEventHostedService)} - Error while adding jobs to HostedService - Exception: {exc.Demystify()}", exc);
            }
        }

        private void AddJobsWithHostedServiceJobName(string jobName, IJobSetting jobSetting)
        {
            switch (jobName)
            {
                case nameof(NotifyFailedEventTimedJob): AddJob<NotifyFailedEventTimedJob>(jobSetting); break;
                case nameof(CleanupEventsEventTimedJob): AddJob<CleanupEventsEventTimedJob>(jobSetting); break;
                default: _logger.LogError($"{nameof(BuyerEventHostedService)} - Coulnd't find implementation of a job for this settings: {jobName} with ID: {jobSetting.JobSettingID}"); break;
            }
        }
    }
}