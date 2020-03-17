using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Models.Metrics;

namespace TQI.WebPortal.Service.IServices
{
    public interface IMetricDataService
    {
        Task<bool> InsertPlayerUnderOvers(List<PlayerOverUnder> playerOverUnders);

        Task<bool> InsertPlayerHeadToHeads(List<PlayerHeadToHead> playerHeadToHeads);

        Task<byte[]> ExportMetricData(string countryCode, string sportCode, DateTime dateTime);
    }
}
