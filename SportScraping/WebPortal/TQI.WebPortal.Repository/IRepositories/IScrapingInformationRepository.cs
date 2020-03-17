using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Entity.Database.BaseRepository;
using TQI.Infrastructure.Entity.Models;

namespace TQI.WebPortal.Repository.IRepositories
{
    public interface IScrapingInformationRepository : IBaseRepository<ScrapingInformation>
    {
        Task<IEnumerable<ScrapingInformation>> GetByDateAndSportCode(DateTime fromDate, DateTime toDate, string sportCode, ScrapeType? scrapeType, int? timeoutSeconds = null);
    }
}
