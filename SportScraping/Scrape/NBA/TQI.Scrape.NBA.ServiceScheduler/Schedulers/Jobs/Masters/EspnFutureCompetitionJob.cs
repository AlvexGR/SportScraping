using System;
using TQI.Infrastructure.Scrape.Scheduler;
using TQI.Infrastructure.Utility;
using TQI.Scrape.NBA.Handler.Handlers.Masters;
using TQI.Scrape.NBA.ServiceScheduler.Service;

namespace TQI.Scrape.NBA.ServiceScheduler.Schedulers.Jobs.Masters
{
    public class EspnFutureCompetitionJob : ScrapeJob
    {
        private const int DaysToScrape = 5;

        public EspnFutureCompetitionJob(WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper) : base(webPortalHelper, scrapeHelper)
        {
            Logger = Helper
                .GetLoggerConfig($@"{NBAScrapingService.BaseLoggerPath}\Scrape\Competition\espn_future-.txt")
                .CreateLogger();
            ProviderType = typeof(EspnFutureCompetition);
        }

        protected override void ModifyProvider()
        {
            Provider.ShouldGetTodayMatches = false;
            ((EspnFutureCompetition)Provider).FromDate = DateTime.Now.AddDays(1);
            ((EspnFutureCompetition)Provider).ToDate = DateTime.Now.AddDays(DaysToScrape);
        }
    }
}
