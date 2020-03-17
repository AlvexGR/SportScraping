using System.Collections.Generic;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Database.BaseRepository;
using TQI.Infrastructure.Entity.Models;

namespace TQI.WebPortal.Repository.IRepositories
{
    public interface ITeamRepository : IBaseRepository<Team>
    {
        Task<IEnumerable<Team>> GetTeams(string sportCode, int? timeoutSeconds = null);
    }
}
