using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using TQI.Infrastructure.Entity.Models;
using TQI.WebPortal.Service.IServices;
using TQI.WebPortal.Service.UnitOfWork;

namespace TQI.WebPortal.Service.Services
{
    public class SimulationService : ISimulationService
    {
        private readonly ILogger _logger;
        private readonly IWebPortalUnitOfWork _unitOfWork;

        public SimulationService(ILogger logger, IWebPortalUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> InsertData(List<TempTableToTest> tableToTests)
        {
            return await _unitOfWork.TempTableToTestRepository.InsertAsync(tableToTests) > 0;
        }

        public async Task<bool> UpdateData(List<TempTableToTest> tableToTests)
        {
            return await _unitOfWork.TempTableToTestRepository.UpdateAsync(tableToTests) > 0;
        }

        public async Task<bool> DeleteData(List<TempTableToTest> tableToTests)
        {
            return await _unitOfWork.TempTableToTestRepository.DeleteAsync(tableToTests) > 0;
        }
    }
}
