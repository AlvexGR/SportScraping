using System.Collections.Generic;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Database.BaseRepository;
using TQI.Infrastructure.Entity.Models;

namespace TQI.WebPortal.Repository.IRepositories
{
    public interface IProviderRepository : IBaseRepository<Provider>
    {
        Task<Provider> GetProviderBySportCode(string sportCode, string providerCode, int? timeoutSeconds = null);

        Task<IEnumerable<Provider>> GetProvidersBySportCode(string sportCode, int? timeoutSeconds = null);

        Task<IEnumerable<Provider>> GetProvidersBySportCodeAndCountryCode(string sportCode, string countryCode, int? timeoutSeconds = null);
    }
}
