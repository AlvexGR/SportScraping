using System.Collections.Generic;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Models;

namespace TQI.WebPortal.Service.IServices
{
    public interface ISimulationService
    {
        Task<bool> InsertData(List<TempTableToTest> tableToTests);
        Task<bool> UpdateData(List<TempTableToTest> tableToTests);
        Task<bool> DeleteData(List<TempTableToTest> tableToTests);
    }
}
