using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Models;

namespace TQI.WebPortal.Service.IServices
{
    public interface IScrapingService
    {
        /// <summary>
        /// Init scraping information for provider queried by sport code
        /// </summary>
        /// <param name="sportCode">Sport Code to match provider</param>
        /// <param name="providerCode">Scraping information about this provider</param>
        /// <returns>Scraping information with status pending for this provider</returns>
        Task<ScrapingInformation> InitScrapingInformation(string sportCode, string providerCode);

        /// <summary>
        /// Update scraping info
        /// </summary>
        /// <param name="scrapingInformation">To update</param>
        /// <returns>Succeed or not</returns>
        Task<bool> UpdateScrapingInformation(ScrapingInformation scrapingInformation);

        /// <summary>
        /// Get list of scraping information by date range, sport code and scrape type
        /// </summary>
        /// <param name="fromDate">from date</param>
        /// <param name="toDate">to date</param>
        /// <param name="sportCode">sport code</param>
        /// <param name="scrapeType">scrape type</param>
        /// <returns>List of scraping information</returns>
        Task<List<ScrapingInformation>> GetByDateAndSportCode(DateTime fromDate, DateTime toDate, string sportCode, int scrapeType);
    }
}
