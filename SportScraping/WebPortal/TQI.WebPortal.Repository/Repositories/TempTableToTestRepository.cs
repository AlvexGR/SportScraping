using TQI.Infrastructure.Entity.Database;
using TQI.Infrastructure.Entity.Database.BaseRepository;
using TQI.Infrastructure.Entity.Models;
using TQI.WebPortal.Repository.IRepositories;

namespace TQI.WebPortal.Repository.Repositories
{
    public class TempTableToTestRepository : BaseRepository<TempTableToTest>, ITempTableToTestRepository
    {
        public TempTableToTestRepository(DbConnectionString dbConnectionString) : base(dbConnectionString)
        {
        }
    }
}
