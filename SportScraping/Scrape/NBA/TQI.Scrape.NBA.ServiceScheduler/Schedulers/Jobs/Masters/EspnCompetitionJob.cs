using System.Threading.Tasks;
using Quartz;
using TQI.Infrastructure.Scrape.Scheduler;
using TQI.Infrastructure.Utility;
using TQI.Scrape.NBA.Handler.Handlers.Masters;
using TQI.Scrape.NBA.ServiceScheduler.Service;

namespace TQI.Scrape.NBA.ServiceScheduler.Schedulers.Jobs.Masters
{
    /// <summary>
    /// Job to scrape ESPN
    /// </summary>
    public class EspnCompetitionJob : ScrapeJob
    {
        public EspnCompetitionJob(WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper) : base(webPortalHelper, scrapeHelper)
        {
            Logger = Helper
                .GetLoggerConfig($@"{NBAScrapingService.BaseLoggerPath}\Scrape\Competition\espn-.txt")
                .CreateLogger();
            ProviderType = typeof(EspnCompetition);
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            await base.Execute(context);

            Logger.Information("Call scheduling for metric data");
            await ScrapeScheduler.Instance.ScheduleMetric();
        }

        protected override void ModifyProvider()
        {
            Provider.ShouldGetTodayMatches = false;
        }
    }
}
