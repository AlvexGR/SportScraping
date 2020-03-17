using TQI.Infrastructure.Scrape.Scheduler;
using TQI.Infrastructure.Utility;
using TQI.Scrape.NBA.Handler.Handlers.Metrics.PlayerOverUnders;
using TQI.Scrape.NBA.ServiceScheduler.Service;

namespace TQI.Scrape.NBA.ServiceScheduler.Schedulers.Jobs.Metrics.PlayerOverUnders
{
    public class BetAmericaPlayerOverUnderJob : ScrapeJob
    {
        public BetAmericaPlayerOverUnderJob(WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(webPortalHelper, scrapeHelper)
        {
            Logger = Helper
                .GetLoggerConfig($@"{NBAScrapingService.BaseLoggerPath}\Scrape\PlayerOverUnder\betamerica-.txt")
                .CreateLogger();
            ProviderType = typeof(BetAmericaPlayerOverUnder);
        }
    }
}
