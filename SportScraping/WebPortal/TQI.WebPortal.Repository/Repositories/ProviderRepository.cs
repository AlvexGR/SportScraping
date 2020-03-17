using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Database;
using TQI.Infrastructure.Entity.Database.BaseRepository;
using TQI.Infrastructure.Entity.Models;
using TQI.WebPortal.Repository.IRepositories;

namespace TQI.WebPortal.Repository.Repositories
{
    public class ProviderRepository : BaseRepository<Provider>, IProviderRepository
    {
        public ProviderRepository(DbConnectionString dbConnectionString) : base(dbConnectionString)
        {
        }

        public async Task<Provider> GetProviderBySportCode(string sportCode, string providerCode, int? timeoutSeconds = null)
        {
            const string sql = @"
SELECT
    *
FROM
    `sports_scraping`.`provider`
WHERE
    `sport_code` = @SportCode
    AND `code` = @ProviderCode;";

            var param = new
            {
                SportCode = sportCode,
                ProviderCode = providerCode
            };
            return (await QueryAsync(sql, param, timeoutSeconds))?.FirstOrDefault();
        }

        public async Task<IEnumerable<Provider>> GetProvidersBySportCode(string sportCode, int? timeoutSeconds = null)
        {
            const string sql = @"SELECT * FROM `sports_scraping`.`provider` WHERE `sport_code` = @SportCode;";

            var param = new
            {
                SportCode = sportCode
            };

            return await QueryAsync(sql, param, timeoutSeconds);
        }

        public async Task<IEnumerable<Provider>> GetProvidersBySportCodeAndCountryCode(string sportCode, string countryCode, int? timeoutSeconds = null)
        {
            const string sql = @"
SELECT *
FROM
    `sports_scraping`.`provider`
WHERE
    `sport_code` = @SportCode
    AND `country_code` = @CountryCode;
";

            var param = new
            {
                SportCode = sportCode,
                CountryCode = countryCode
            };

            return await QueryAsync(sql, param, timeoutSeconds);
        }
    }
}
