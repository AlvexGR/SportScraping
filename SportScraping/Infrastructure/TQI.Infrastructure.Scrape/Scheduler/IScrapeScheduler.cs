using System.Threading.Tasks;

namespace TQI.Infrastructure.Scrape.Scheduler
{
    public interface IScrapeScheduler
    {
        /// <summary>
        /// Register jobs and dependencies
        /// </summary>
        Task RegisterJobsAndDependencies();

        /// <summary>
        /// Scheduling for today matches
        /// </summary>
        /// <param name="providerCode">Competition provider</param>
        /// <param name="cronTime">Cron time expression</param>
        Task ScheduleTodayMatches(string providerCode, string cronTime = "");

        /// <summary>
        /// Scheduling for future matches
        /// </summary>
        /// // <param name="providerCode">Competition provider</param>
        /// /// <param name="cronTime">Cron time expression</param>
        Task ScheduleFutureMatches(string providerCode, string cronTime = "");

        /// <summary>
        /// Scheduling for all active metric providers
        /// </summary>
        Task ScheduleMetric();

        /// <summary>
        /// Scheduling for a given provider at cron time
        /// </summary>
        /// <param name="providerCode">Provider</param>
        /// <param name="cronTime">Cron time expression</param>
        Task Schedule(string providerCode, string cronTime = "");

        /// <summary>
        /// Stop a specific provider at cron time from Schedule method
        /// </summary>
        /// <param name="providerCode"></param>
        /// <param name="cronTime"></param>
        Task Stop(string providerCode, string cronTime = "");
    }
}
