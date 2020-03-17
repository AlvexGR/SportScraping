using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using TQI.Infrastructure.Entity.Models;
using TQI.WebPortal.Service.IServices;
using TQI.WebPortal.Service.UnitOfWork;

namespace TQI.WebPortal.Service.Services
{
    public class SportService : ISportService
    {
        private readonly ILogger _logger;
        private readonly IWebPortalUnitOfWork _unitOfWork;

        public SportService(ILogger logger, IWebPortalUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Sport>> GetAll()
        {
            var result = await _unitOfWork.SportRepository.GetAll();
            _logger.Information($"Get all sports result: {JsonConvert.SerializeObject(result)}");
            return result?.ToList();
        }

        public async Task<List<Provider>> GetProviders(string sportCode)
        {
            if (string.IsNullOrEmpty(sportCode))
            {
                throw new ArgumentException("SportCode is empty");
            }

            var result = await _unitOfWork.ProviderRepository.GetProvidersBySportCode(sportCode);
            _logger.Information($"Get providers result: {JsonConvert.SerializeObject(result)}");
            return result?.ToList();
        }
    }
}
