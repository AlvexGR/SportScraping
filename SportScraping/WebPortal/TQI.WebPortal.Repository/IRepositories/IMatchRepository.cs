using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Database.BaseRepository;
using TQI.Infrastructure.Entity.Models;

namespace TQI.WebPortal.Repository.IRepositories
{
    public interface IMatchRepository : IBaseRepository<Match>
    {
        Task<IEnumerable<Match>> GetMatchesByGameCodes(IEnumerable<string> gameCodes, int? timeoutSeconds = null);

        Task<IEnumerable<Match>> GetMatches(string sportCode, DateTime fromDate, DateTime toDate, int? timeoutSeconds = null);
    }
}
