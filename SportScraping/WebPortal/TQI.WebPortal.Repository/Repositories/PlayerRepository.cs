using System.Collections.Generic;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Database;
using TQI.Infrastructure.Entity.Database.BaseRepository;
using TQI.Infrastructure.Entity.Models;
using TQI.WebPortal.Repository.IRepositories;

namespace TQI.WebPortal.Repository.Repositories
{
    public class PlayerRepository : BaseRepository<Player>, IPlayerRepository
    {
        public PlayerRepository(DbConnectionString dbConnectionString) : base(dbConnectionString)
        {
        }

        public async Task<IEnumerable<Player>> GetPlayersBySourceId(IEnumerable<string> sourceIds, int? timeoutSeconds = null)
        {
            const string sql = @"SELECT * FROM `sports_scraping`.`player` WHERE `source_id` IN @SourceIds";
            var param = new
            {
                SourceIds = sourceIds
            };
            return await QueryAsync(sql, param, timeoutSeconds);
        }
    }
}
