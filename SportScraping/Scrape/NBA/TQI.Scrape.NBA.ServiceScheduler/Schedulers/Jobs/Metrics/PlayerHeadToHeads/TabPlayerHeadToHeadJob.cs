using TQI.Infrastructure.Scrape.Scheduler;
using TQI.Infrastructure.Utility;
using TQI.Scrape.NBA.Handler.Handlers.Metrics.PlayerHeadToHeads;
using TQI.Scrape.NBA.ServiceScheduler.Service;

namespace TQI.Scrape.NBA.ServiceScheduler.Schedulers.Jobs.Metrics.PlayerHeadToHeads
{
    public class TabPlayerHeadToHeadJob : ScrapeJob
    {
        public TabPlayerHeadToHeadJob(WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(webPortalHelper, scrapeHelper)
        {
            Logger = Helper
                .GetLoggerConfig($@"{NBAScrapingService.BaseLoggerPath}\Scrape\PlayerHeadToHead\tab-.txt")
                .CreateLogger();
            ProviderType = typeof(TabPlayerHeadToHead);
        }
    }
}
