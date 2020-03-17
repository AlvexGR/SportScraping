using System;
using System.Threading.Tasks;
using Quartz;
using Serilog;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Scrape.Handler;
using TQI.Infrastructure.Utility;

namespace TQI.Infrastructure.Scrape.Scheduler
{
    public abstract class ScrapeJob : IJob
    {
        protected readonly WebPortalHelper WebPortalHelper;
        protected readonly ScrapeHelper ScrapeHelper;
        protected ILogger Logger;
        protected IScrapeHandler Provider;
        protected Type ProviderType;

        protected ScrapeJob(WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
        {
            WebPortalHelper = webPortalHelper;
            ScrapeHelper = scrapeHelper;
        }

        public virtual async Task Execute(IJobExecutionContext context)
        {
            Logger.Information("Creating provider instance");
            Provider = await ScrapeHandlerFactory.CreateAsync(ProviderType, Logger, WebPortalHelper, ScrapeHelper);

            ModifyProvider();

            var success = false;
            int i;
            for (i = 0; i < Constants.RetryAttempt; i++)
            {
                Logger.Information($"Scrape attempt: {i + 1}");
                if (await Provider.Scrape())
                {
                    success = true;
                    break;
                }
                await Task.Delay(Constants.RetryAfter);
            }

            if (success)
            {
                Logger.Information($"Scrape succeeded after {i + 1} attempt(s)");
            }
            else
            {
                Logger.Error($"Scrape failed after {Constants.RetryAttempt} attempts");
            }
        }

        /// <summary>
        /// Allow derived classes to modify provider before scraping
        /// </summary>
        protected virtual void ModifyProvider() { }
    }
}
