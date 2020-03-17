using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Quartz;
using Serilog;
using TQI.Infrastructure.Scrape.Service;
using TQI.Infrastructure.Utility;

namespace TQI.Infrastructure.Scrape.Scheduler
{
    public abstract class ScrapeScheduler : IScrapeScheduler
    {
        /// <summary>
        /// Singleton Instance
        /// </summary>
        public static IScrapeScheduler Instance;

        protected readonly ILogger Logger;
        protected readonly WebPortalHelper WebPortalHelper;
        protected readonly CronBuilder CronBuilder;
        protected IServiceProvider ServiceProvider;

        protected ScrapeScheduler(ILogger logger)
        {
            Logger = logger;
            WebPortalHelper = new WebPortalHelper(new HttpClient());
            CronBuilder = new CronBuilder();

            // Inject
            WebPortalHelper.Logger = Logger;
        }

        /// <summary>
        /// Derived classes must implement to create ScrapeSchedulerJob type for scheduler
        /// Must follow {fullNamespace}{providerCode}Job convention
        /// </summary>
        /// <param name="providerCode">Type name</param>
        /// <returns>Type desired from derived class</returns>
        protected abstract Type JobTypeCreator(string providerCode);

        public virtual async Task RegisterJobsAndDependencies()
        {
            var scheduler = await BaseScrapingService.SchedulerFactory.GetScheduler();

            // Add job factory
            Logger.Information("Create job factory");
            scheduler.JobFactory = new ScrapeJobFactory(ServiceProvider);
        }

        public virtual async Task ScheduleTodayMatches(string providerCode, string cronTime = "")
        {
            if (string.IsNullOrEmpty(providerCode))
            {
                Logger.Error("Input provider is null");
                return;
            }

            try
            {
                // Don't start scraping if inactive
                if (Helper.GetActiveProviders().All(x => x.Code != providerCode))
                {
                    Logger.Warning("Setting for today matches scraping is OFF");
                    // Start metric for any active provider
                    if (Helper.GetActiveProviders().Any(x => x.IsMetric))
                    {
                        await ScheduleMetric();
                    }

                    return;
                }

                Logger.Information("Start scheduling today master data scraping");

                var scheduler = await BaseScrapingService.SchedulerFactory.GetScheduler();

                // Add jobs
                Logger.Information("Start adding jobs");

                var todayMatchesJobKey =
                    new JobKey(Helper.GetJobKey(providerCode));

                var todayMatchesTriggerKey =
                    new TriggerKey(Helper.GetTriggerKey(providerCode));

                // Stop first before start new one
                if (await scheduler.CheckExists(todayMatchesJobKey))
                {
                    Logger.Information("Delete previous today matches job");
                    await scheduler.DeleteJob(todayMatchesJobKey);
                }

                var todayMatchesJob = JobBuilder.Create(JobTypeCreator(providerCode)).WithIdentity(todayMatchesJobKey)
                    .Build();

                var triggerBuilder = TriggerBuilder.Create().WithIdentity(todayMatchesTriggerKey).StartNow();
                if (!string.IsNullOrEmpty(cronTime))
                {
                    triggerBuilder.WithCronSchedule(cronTime);
                }

                var todayMatchesTrigger = triggerBuilder.Build();

                Logger.Information($"ScheduleTodayMatches today matches scraping with cron {cronTime}");
                await scheduler.ScheduleJob(todayMatchesJob, todayMatchesTrigger);

                Logger.Information("Start scheduler");
                await scheduler.Start();

                Logger.Information("Finish scheduling today master data scraping");
            }
            catch (Exception ex)
            {
                Logger.Error($"Scheduling for today matches error: {ex}");
            }
        }

        public virtual async Task ScheduleFutureMatches(string providerCode, string cronTime = "")
        {
            if (string.IsNullOrEmpty(providerCode))
            {
                Logger.Error("Input provider is null");
                return;
            }

            try
            {
                // Don't start scraping if inactive
                if (Helper.GetActiveProviders().All(x => x.Code != providerCode))
                {
                    Logger.Warning("Setting for future matches scraping is OFF");
                    return;
                }

                Logger.Information("Start scheduling future master data scraping");

                var scheduler = await BaseScrapingService.SchedulerFactory.GetScheduler();

                // Add jobs
                Logger.Information("Start adding job");

                var futureMatchesJobKey =
                    new JobKey(Helper.GetJobKey(providerCode));

                var futureMatchesTriggerKey =
                    new TriggerKey(Helper.GetTriggerKey(providerCode));

                // Stop first before start new one
                if (await scheduler.CheckExists(futureMatchesJobKey))
                {
                    Logger.Information("Delete previous future matches job");
                    await scheduler.DeleteJob(futureMatchesJobKey);
                }

                var futureMatchesJob = JobBuilder.Create(JobTypeCreator(providerCode)).WithIdentity(futureMatchesJobKey).Build();

                var triggerBuilder = TriggerBuilder.Create().WithIdentity(futureMatchesTriggerKey).StartNow();
                if (!string.IsNullOrEmpty(cronTime))
                {
                    triggerBuilder.WithCronSchedule(cronTime);
                }

                var futureMatchesTrigger = triggerBuilder.Build();

                // ScheduleTodayMatches job
                Logger.Information($"ScheduleTodayMatches future matches scraping with cron {cronTime}");
                await scheduler.ScheduleJob(futureMatchesJob, futureMatchesTrigger);

                Logger.Information("Start scheduler");

                await scheduler.Start();

                Logger.Information("Finish scheduling future master data scraping");
            }
            catch (Exception ex)
            {
                Logger.Error($"Scheduling for future matches error: {ex}");
            }
        }

        public virtual async Task ScheduleMetric()
        {
            try
            {
                Logger.Information("Start scheduling metric data scraping");

                var cronTime = await ConfigureCronTimeForMetricScheduling();
                if (string.IsNullOrEmpty(cronTime))
                {
                    Logger.Warning("Cron is empty");
                    return;
                }

                var scheduler = await BaseScrapingService.SchedulerFactory.GetScheduler();

                // Add jobs
                Logger.Information("Start adding jobs");

                var jobDict = new Dictionary<IJobDetail, IReadOnlyCollection<ITrigger>>();
                foreach (var provider in Helper.GetActiveProviders()
                    .Where(x => x.IsMetric))
                {
                    var metricJobKey =
                        new JobKey(Helper.GetJobKey(provider.Code));

                    // Stop first before start new one
                    if (await scheduler.CheckExists(metricJobKey))
                    {
                        Logger.Information($"Delete previous job of {provider.Code}");
                        await scheduler.DeleteJob(metricJobKey);
                    }

                    var metricJob = JobBuilder.Create(JobTypeCreator(provider.Code)).WithIdentity(metricJobKey).Build();

                    var triggers = new List<ITrigger>();

                    var metricTriggerKey =
                        new TriggerKey(Helper.GetTriggerKey(provider.Code));

                    var metricTrigger = TriggerBuilder.Create()
                        .WithIdentity(metricTriggerKey)
                        .StartNow()
                        .WithCronSchedule(cronTime)
                        .Build();

                    Logger.Information($"Schedule for {provider.Code} at {cronTime}");

                    triggers.Add(metricTrigger);
                    jobDict.Add(metricJob, triggers);
                }

                if (jobDict.Any())
                {
                    await scheduler.ScheduleJobs(jobDict, true);
                    Logger.Information("Start scheduler");
                    await scheduler.Start();
                }

                Logger.Information("Finish scheduling metric data scraping");
            }
            catch (Exception ex)
            {
                Logger.Error($"Scheduling metric data error: {ex}");
            }
        }

        /// <summary>
        /// Config cron time for scheduling metric
        /// </summary>
        /// <returns>Cron time expression</returns>
        protected abstract Task<string> ConfigureCronTimeForMetricScheduling();

        public virtual async Task Schedule(string providerCode, string cronTime = "")
        {
            if (string.IsNullOrEmpty(providerCode))
            {
                Logger.Error("Input provider is null");
                return;
            }

            try
            {
                if (Helper.GetActiveProviders().All(x => x.Code != providerCode))
                {
                    Logger.Information($"Setting for {providerCode} scraping is OFF");
                    return;
                }

                Logger.Information($"Start scheduling data scraping for {providerCode} " +
                                    $"at: {cronTime}");

                var scheduler = await BaseScrapingService.SchedulerFactory.GetScheduler();

                // Add jobs
                Logger.Information("Start adding jobs");

                var providerJobKey =
                    new JobKey(Helper.GetJobKey($"{providerCode}-{cronTime}"));
                var providerTriggerKey =
                    new TriggerKey(Helper.GetTriggerKey($"{providerCode}-{cronTime}"));

                // Stop first before start new one
                if (await scheduler.CheckExists(providerJobKey))
                {
                    Logger.Information("Delete previous job");
                    await scheduler.DeleteJob(providerJobKey);
                }

                var providerJob = JobBuilder.Create(JobTypeCreator(providerCode)).WithIdentity(providerJobKey).Build();

                var triggerBuilder = TriggerBuilder.Create().WithIdentity(providerTriggerKey).StartNow();
                if (!string.IsNullOrEmpty(cronTime))
                {
                    triggerBuilder.WithCronSchedule(cronTime);
                }

                var providerTrigger = triggerBuilder.Build();

                //Logger.Information($"ScheduleTodayMatches today matches scraping with cron {ScrapeTodayMatchesCronExpression}");
                await scheduler.ScheduleJob(providerJob, providerTrigger);

                Logger.Information("Start scheduler");
                await scheduler.Start();

                Logger.Information($"Finish scheduling data scraping for {providerCode} " +
                                    $"at {cronTime}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Scheduling for {providerCode} at " +
                              $"{cronTime} " +
                              $"error: {ex}");
            }
        }

        public virtual async Task Stop(string providerCode, string cronTime = "")
        {
            if (string.IsNullOrEmpty(providerCode))
            {
                Logger.Error("Input provider is null");
                return;
            }

            try
            {
                Logger.Information($"Stop {providerCode} " +
                                    $"at: {cronTime}");

                var scheduler = await BaseScrapingService.SchedulerFactory.GetScheduler();

                var providerJobKey =
                    new JobKey(Helper.GetJobKey($"{providerCode}-{cronTime}"));

                // Stop first before start new one
                if (await scheduler.CheckExists(providerJobKey))
                {
                    Logger.Information("Delete job");
                    await scheduler.DeleteJob(providerJobKey);
                }
                else
                {
                    Logger.Information("No job found");
                }

                Logger.Information($"Scraping data from {providerCode} " +
                                    $"at {cronTime} stopped");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while stopping {providerCode} at " +
                              $"{cronTime} " +
                              $"error: {ex}");
            }
        }
    }
}
