using System.Collections.Generic;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Database;
using TQI.Infrastructure.Entity.Database.BaseRepository;
using TQI.Infrastructure.Entity.Models;
using TQI.WebPortal.Repository.IRepositories;

namespace TQI.WebPortal.Repository.Repositories
{
    public class SportRepository : BaseRepository<Sport>, ISportRepository
    {
        public SportRepository(DbConnectionString dbConnectionString) : base(dbConnectionString)
        {
        }

        public async Task<IEnumerable<Sport>> GetAll(int? timeoutSeconds = null)
        {
            const string sql = @"SELECT * FROM `sports_scraping`.`sport`";
            return await QueryAsync(sql, timeoutSeconds: timeoutSeconds);
        }
    }
}
