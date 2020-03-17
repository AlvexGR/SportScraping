using System.Collections.Generic;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Database.BaseRepository;
using TQI.Infrastructure.Entity.Models;

namespace TQI.WebPortal.Repository.IRepositories
{
    public interface IPlayerRepository : IBaseRepository<Player>
    {
        Task<IEnumerable<Player>> GetPlayersBySourceId(IEnumerable<string> sourceIds, int? timeoutSeconds = null);
    }
}
