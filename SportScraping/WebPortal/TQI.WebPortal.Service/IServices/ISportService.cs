using System.Collections.Generic;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Models;

namespace TQI.WebPortal.Service.IServices
{
    public interface ISportService
    {
        Task<List<Sport>> GetAll();

        Task<List<Provider>> GetProviders(string sportCode);
    }
}
