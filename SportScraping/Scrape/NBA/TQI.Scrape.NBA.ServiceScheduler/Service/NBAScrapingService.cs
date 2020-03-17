using System;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Scrape.Scheduler;
using TQI.Infrastructure.Scrape.Service;
using TQI.Infrastructure.Utility;
using TQI.Scrape.NBA.Handler.Handlers.Masters;
using TQI.Scrape.NBA.ServiceScheduler.Schedulers;
using TQI.Scrape.NBA.ServiceScheduler.WcfContract;

namespace TQI.Scrape.NBA.ServiceScheduler.Service
{
    public class NBAScrapingService : BaseScrapingService
    {
        public static readonly string BaseLoggerPath = $@"{Constants.BaseLoggerPath}\NBA";
        public NBAScrapingService()
        {
            ServiceName = "NBA Scraping Service";
            Logger = Helper
                .GetLoggerConfig($@"{BaseLoggerPath}\Service\service-.txt")
                .CreateLogger();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Logger.Information("Start scraping service");

                CreateServiceHost<NBAScrapingContract>();
                base.OnStart(args);

                // Create logger for scheduler
                var scheduleLogger = Helper
                    .GetLoggerConfig($@"{BaseLoggerPath}\Scheduler\schedule-.txt")
                    .CreateLogger();

                Logger.Information("Init data scraping scheduler");
                ScrapeScheduler.Instance = new NBAScheduler(scheduleLogger);
                ScrapeScheduler.Instance.RegisterJobsAndDependencies().Wait();

                Logger.Information("Start scraping scheduler");
                ScrapeScheduler.Instance.ScheduleTodayMatches(nameof(EspnCompetition)).Wait();
                ScrapeScheduler.Instance.ScheduleFutureMatches(nameof(EspnFutureCompetition)).Wait();

                Logger.Information("Scraping service started successfully");
            }
            catch (Exception ex)
            {
                Logger.Error($"Cannot start scraping service: {ex}");
            }
        }

        protected override void OnStop()
        {
            try
            {
                Logger.Information("Stop scraping service");
                base.OnStop();
                Logger.Information("Scraping service stopped successfully");
            }
            catch (Exception ex)
            {
                Logger.Error($"Cannot stop scraping service: {ex}");
            }
        }

        public static void Main()
        {
#if DEBUG
            var nbaScrapingService = new NBAScrapingService();
            nbaScrapingService.RunDebug();
#else
            Run(new NBAScrapingService());
#endif
        }
    }
}
