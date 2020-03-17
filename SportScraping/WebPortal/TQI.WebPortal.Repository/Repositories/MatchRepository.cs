using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Database;
using TQI.Infrastructure.Entity.Database.BaseRepository;
using TQI.Infrastructure.Entity.Models;
using TQI.WebPortal.Repository.IRepositories;

namespace TQI.WebPortal.Repository.Repositories
{
    public class MatchRepository : BaseRepository<Match>, IMatchRepository
    {
        public MatchRepository(DbConnectionString dbConnectionString) : base(dbConnectionString)
        {
        }

        public async Task<IEnumerable<Match>> GetMatchesByGameCodes(IEnumerable<string> gameCodes, int? timeoutSeconds = null)
        {
            const string sql = @"SELECT * FROM `sports_scraping`.`match` WHERE `game_code` IN @GameCodes";
            var param = new
            {
                GameCodes = gameCodes
            };
            return await QueryAsync(sql, param, timeoutSeconds);
        }

        public async Task<IEnumerable<Match>> GetMatches(string sportCode, DateTime fromDate, DateTime toDate, int? timeoutSeconds = null)
        {
            const string sql = @"
SELECT
    *
FROM 
    `sports_scraping`.`match`
WHERE
    `sport_code` = @SportCode
    AND `start_time` BETWEEN @FromDate AND @ToDate;";

            var param = new
            {
                SportCode = sportCode,
                FromDate = fromDate,
                ToDate = toDate
            };
            return await QueryAsync(sql, param, timeoutSeconds);
        }
    }
}
