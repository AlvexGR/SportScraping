using System.Collections.Generic;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Database.BaseRepository;
using TQI.Infrastructure.Entity.Models;

namespace TQI.WebPortal.Repository.IRepositories
{
    public interface ISportRepository : IBaseRepository<Sport>
    {
        Task<IEnumerable<Sport>> GetAll(int? timeoutSeconds = null);
    }
}
