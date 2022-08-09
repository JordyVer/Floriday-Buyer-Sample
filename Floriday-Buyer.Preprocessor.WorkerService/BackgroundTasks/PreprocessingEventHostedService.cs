using Axerrio.BB.AspNetCore.Quartz.Infrastructure.Hosting.HostedServices;
using Axerrio.BB.AspNetCore.Quartz.Infrastructure.SchedulerListener;
using Axerrio.BB.AspNetCore.Quartz.Model;
using Axerrio.BB.DDD.Job.BackgroundTasks.TimedJobs;
using Axerrio.BB.DDD.Job.Domain.Aggregates.JobSettingAggregate.Abstractions;
using EnsureThat;
using Quartz.Spi;
using System.Diagnostics;

namespace Floriday_Buyer.Preprocessor.WorkerService.BackgroundTasks
{
    public class PreprocessingEventHostedService : HostedService
    {
        private readonly ILogger<HostedService> _logger;

        public async Task RestartScheduler(CancellationToken stoppingToken)
        {
            using (_logger.BeginScope("{Context} -", $"{nameof(PreprocessingEventHostedService)}"))
            {
                _logger.LogInformation("Stopping the hosted service");

                await base.StopAsync(stoppingToken);

                _logger.LogInformation("Hosted service was stopped, attempting to restart now");

                await base.ExecuteAsync(stoppingToken);

                _logger.LogInformation("HostedService was started");
            }
        }

        public PreprocessingEventHostedService(ILogger<HostedService> logger, IJobFactory jobFactory, IJobSettingRepository jobSettingRepository, QuartzSchedulerListener listener) : base(logger, jobFactory, listener)
        {
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));

            var jobSettings = jobSettingRepository.GetAllActiveAsync().Result;

            if (jobSettings.Count() == 0)
            {
                logger.LogError($"{nameof(PreprocessingEventHostedService)} - Couldn't retrieve settings from the database");
                return;
            }

            try
            {
                foreach (var jobSetting in jobSettings)
                {
                    if (jobSetting == null)
                    {
                        logger.LogError($"{nameof(PreprocessingEventHostedService)} - Couldn't register job: {jobSetting.Name} with ID: {jobSetting.JobSettingID}, because job settings were not found!");
                        continue;
                    }

                    AddJobsWithHostedServiceJobName(jobSetting.Name, jobSetting);
                }
            }
            catch (Exception exc)
            {
                logger.LogError($"{nameof(PreprocessingEventHostedService)} - Error while adding jobs to HostedService - Exception: {exc.Demystify()}", exc);
            }
        }

        private void AddJobsWithHostedServiceJobName(string jobName, IJobSetting jobSetting)
        {
            switch (jobName)
            {
                case nameof(WorkServiceEventTimedJob): AddJob<WorkServiceEventTimedJob>(jobSetting); break;
                case nameof(JobHistoryCleanupTimedJob): AddJob<JobHistoryCleanupTimedJob>(jobSetting); break;
                default: _logger.LogError($"{nameof(PreprocessingEventHostedService)} - Coulnd't find implementation of a job for this settings: {jobName} with ID: {jobSetting.JobSettingID}"); break;
            }
        }
    }
}