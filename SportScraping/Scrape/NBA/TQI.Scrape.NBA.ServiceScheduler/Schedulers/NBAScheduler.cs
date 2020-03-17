using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Scrape.Scheduler;
using TQI.Infrastructure.Utility;
using TQI.Scrape.NBA.ServiceScheduler.Schedulers.Jobs.Masters;
using TQI.Scrape.NBA.ServiceScheduler.Schedulers.Jobs.Metrics.PlayerHeadToHeads;
using TQI.Scrape.NBA.ServiceScheduler.Schedulers.Jobs.Metrics.PlayerOverUnders;

namespace TQI.Scrape.NBA.ServiceScheduler.Schedulers
{
    public class NBAScheduler : ScrapeScheduler
    {
        private const string ScrapeFutureMatchesCronExpression = "0 0 16 ? JAN,FEB,MAR,APR,MAY,JUN,SEP,OCT,NOV,DEC * *";
        //private const string ScrapeFutureMatchesCronExpression = "0/20 * * ? * * *";
        private const string ScrapeTodayMatchesCronExpression = "0 0 4 ? JAN,FEB,MAR,APR,MAY,JUN,SEP,OCT,NOV,DEC * *";
        //private const string ScrapeTodayMatchesCronExpression = "0 0-59/2 * ? * * *";

        public NBAScheduler(ILogger logger) : base(logger)
        {
        }

        public override async Task RegisterJobsAndDependencies()
        {
            Logger.Information("Register jobs and dependencies");
            ServiceProvider = new ServiceCollection()
                #region Competition

                .AddScoped<EspnCompetitionJob>()
                .AddScoped<EspnFutureCompetitionJob>()

                #endregion
                #region Over / Under

                .AddScoped<PalmerbetPlayerOverUnderJob>()
                .AddScoped<BetEasyPlayerOverUnderJob>()
                .AddScoped<PointsBetPlayerOverUnderJob>()
                .AddScoped<NedsPlayerOverUnderJob>()
                .AddScoped<Bet365PlayerOverUnderJob>()
                .AddScoped<NextBetPlayerOverUnderJob>()
                .AddScoped<TabPlayerOverUnderJob>()
                .AddScoped<KambiBePlayerOverUnderJob>()
                .AddScoped<BorgataonlinePlayerOverUnderJob>()
                .AddScoped<BovadaPlayerOverUnderJob>()
                .AddScoped<FiveDimesPlayerOverUnderJob>()
                .AddScoped<TwoTwoBetPlayerOverUnderJob>()
                .AddScoped<BetAmericaPlayerOverUnderJob>()
                .AddScoped<BetVictorPlayerOverUnderJob>()
                .AddScoped<SportsBetPlayerOverUnderJob>()
                .AddScoped<TopSportPlayerOverUnderJob>()
                .AddScoped<UbetPlayerOverUnderJob>()

                #endregion
                #region Head To Head

                .AddScoped<TabPlayerHeadToHeadJob>()

            #endregion
                #region Dependency

                .AddTransient<WebPortalHelper>()
                .AddTransient<ScrapeHelper>()
                .AddSingleton<HttpClient>()
                .AddSingleton<WebClient>()

                #endregion
                .BuildServiceProvider();

            await base.RegisterJobsAndDependencies();

            Logger.Information("Register jobs and dependencies complete");
        }

        protected override Type JobTypeCreator(string providerCode)
        {
            // Namespace contains scrape scheduler jobs
            const string defaultNamespace = "TQI.Scrape.NBA.ServiceScheduler.Schedulers.Jobs";

            var fullNamespace = string.Empty;
            if (providerCode.Contains("Competition"))
            {
                fullNamespace = $"{defaultNamespace}.Masters";
            }
            else if (providerCode.Contains("PlayerOverUnder"))
            {
                fullNamespace = $"{defaultNamespace}.Metrics.PlayerOverUnders";
            }
            else if (providerCode.Contains("PlayerHeadToHead"))
            {
                fullNamespace = $"{defaultNamespace}.Metrics.PlayerHeadToHeads";
            }

            return Type.GetType($"{fullNamespace}.{providerCode}Job");
        }

        public override async Task ScheduleTodayMatches(string providerCode, string cronTime = "")
        {
            await base.ScheduleTodayMatches(providerCode,
                !string.IsNullOrEmpty(cronTime) ? cronTime : ScrapeTodayMatchesCronExpression);
        }

        public override async Task ScheduleFutureMatches(string providerCode, string cronTime = "")
        {
            await base.ScheduleFutureMatches(providerCode,
                !string.IsNullOrEmpty(cronTime) ? cronTime : ScrapeFutureMatchesCronExpression);
        }

        protected override async Task<string> ConfigureCronTimeForMetricScheduling()
        {
            var todayMatches = await WebPortalHelper.GetSingletonTodayMatches();
            if (todayMatches == null || todayMatches.Count == 0)
            {
                Logger.Warning("No matches to schedule metric scraping");
                return string.Empty;
            }
            var fromHour = todayMatches.Min(x => x.StartTime.Hour - Constants.ScrapingMetricHourBefore); // Get min hour
            var toHour = todayMatches.Max(x => x.StartTime.Hour); // get max hour

            var cronTime = CronBuilder
                .AtSpecificSecond(0)
                .EveryMinutesStartingAt(30, 0)
                .EveryHourBetween(fromHour, toHour)
                .AtSpecificDayOfMonth(DateTime.Now.Day)
                .AtSpecificMonth(DateTime.Now.Month)
                .AtSpecificYear(DateTime.Now.Year)
                .Build();

            return cronTime;
        }
    }
}
