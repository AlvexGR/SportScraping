using System;
using System.Threading.Tasks;
using Serilog;
using TQI.Infrastructure.Utility;

namespace TQI.Infrastructure.Scrape.Handler
{
    public static class ScrapeHandlerFactory
    {
        public static async Task<IScrapeHandler> CreateAsync(Type providerType)
        {
            var instance = (IScrapeHandler) Activator.CreateInstance(providerType);
            await instance.Initialize(providerType);
            return instance;
        }

        public static async Task<IScrapeHandler> CreateAsync(Type providerType, ILogger logger,
            WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
        {
            var instance = (IScrapeHandler) Activator.CreateInstance(providerType, logger, webPortalHelper, scrapeHelper);
            await instance.Initialize(providerType);
            return instance;
        }

        public static IScrapeHandler Create<T>() where T : IScrapeHandler, new()
        {
            return new T();
        }
    }
}
