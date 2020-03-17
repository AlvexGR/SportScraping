using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Database.BaseRepository;
using TQI.Infrastructure.Entity.Models.Metrics;

namespace TQI.WebPortal.Repository.IRepositories
{
    public interface IPlayerHeadToHeadRepository : IBaseRepository<PlayerHeadToHead>
    {
        Task<IEnumerable<PlayerHeadToHead>> GetByCountryAndSportCode(string countryCode, string sportCode, DateTime dateTime, int? timeoutSeconds = null);
    }
}
