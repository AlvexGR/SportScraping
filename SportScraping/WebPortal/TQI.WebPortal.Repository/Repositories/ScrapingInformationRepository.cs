using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Entity.Database;
using TQI.Infrastructure.Entity.Database.BaseRepository;
using TQI.Infrastructure.Entity.Models;
using TQI.WebPortal.Repository.IRepositories;

namespace TQI.WebPortal.Repository.Repositories
{
    public class ScrapingInformationRepository : BaseRepository<ScrapingInformation>, IScrapingInformationRepository
    {
        public ScrapingInformationRepository(DbConnectionString dbConnectionString) : base(dbConnectionString)
        {
        }

        public async Task<IEnumerable<ScrapingInformation>> GetByDateAndSportCode(DateTime fromDate, DateTime toDate, string sportCode, ScrapeType? scrapeType,
            int? timeoutSeconds = null)
        {
            var sql = $@"
SELECT
    *
FROM
    `sports_scraping`.`scraping_information` AS si
    JOIN `sports_scraping`.`provider` AS p ON si.`provider_id` = p.`id`
WHERE
    p.`sport_code` = @SportCode
    {(scrapeType != null ? "AND p.`scrape_type` = @ScrapeType" : string.Empty)}
    AND si.`scrape_time` BETWEEN @FromDate AND @ToDate;
";
            var param = new
            {
                SportCode = sportCode,
                ScrapeType = scrapeType,
                FromDate = fromDate,
                ToDate = toDate
            };

            IEnumerable<ScrapingInformation> result;
            using (var connection = GetConnection())
            {
                connection.Open();
                result = await connection.QueryAsync<ScrapingInformation, Provider, ScrapingInformation>(sql, (scrapeInfo, provider) =>
                {
                    scrapeInfo.Provider = provider;
                    return scrapeInfo;
                }, param, commandTimeout: timeoutSeconds);
                connection.Close();
            }

            return result;
        }
    }
}
