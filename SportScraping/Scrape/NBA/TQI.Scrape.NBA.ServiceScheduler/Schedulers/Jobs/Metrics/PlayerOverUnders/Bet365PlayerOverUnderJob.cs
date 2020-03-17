using TQI.Infrastructure.Scrape.Scheduler;
using TQI.Infrastructure.Utility;
using TQI.Scrape.NBA.Handler.Handlers.Metrics.PlayerOverUnders;
using TQI.Scrape.NBA.ServiceScheduler.Service;

namespace TQI.Scrape.NBA.ServiceScheduler.Schedulers.Jobs.Metrics.PlayerOverUnders
{
    public class Bet365PlayerOverUnderJob : ScrapeJob
    {
        public Bet365PlayerOverUnderJob(WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper) : base(webPortalHelper, scrapeHelper)
        {
            Logger = Helper
                .GetLoggerConfig($@"{NBAScrapingService.BaseLoggerPath}\Scrape\PlayerOverUnder\bet365-.txt")
                .CreateLogger();
            ProviderType = typeof(Bet365PlayerOverUnder);
        }
    }
}
