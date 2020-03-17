using System;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Models;

namespace TQI.Infrastructure.Scrape.Handler
{
    public interface IScrapeHandler
    {
        /// <summary>
        /// Should get today matches
        /// </summary>
        bool ShouldGetTodayMatches { get; set; }

        /// <summary>
        /// Init required data to scrape
        /// </summary>
        Task Initialize(Type providerType);

        /// <summary>
        /// Scrape data
        /// </summary>
        /// <returns>Scrape succeeded or not</returns>
        Task<bool> Scrape();

        /// <summary>
        /// Get current scraping info
        /// </summary>
        /// <returns>Current scraping info</returns>
        ScrapingInformation GetScrapingInformation();
    }
}
