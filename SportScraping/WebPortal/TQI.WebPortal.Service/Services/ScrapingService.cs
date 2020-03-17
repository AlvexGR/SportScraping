using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Entity.Models;
using TQI.Infrastructure.Utility;
using TQI.WebPortal.Service.IServices;
using TQI.WebPortal.Service.UnitOfWork;

namespace TQI.WebPortal.Service.Services
{
    public class ScrapingService : IScrapingService
    {
        private readonly ILogger _logger;
        private readonly IWebPortalUnitOfWork _unitOfWork;

        public ScrapingService(ILogger logger, IWebPortalUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ScrapingInformation> InitScrapingInformation(string sportCode, string providerCode)
        {
            var provider = await _unitOfWork.ProviderRepository.GetProviderBySportCode(sportCode, providerCode);
            if (provider == null)
            {
                _logger.Error($"Can't find any provider {providerCode} with SportCode: {sportCode}");
                return null;
            }
            var scrapingInformation = new ScrapingInformation
            {
                ProviderId = provider.Id,
                ScrapeTime = DateTime.Now,
                Progress = 0,
                ProgressExplanation = "Not yet started",
                ScrapeStatus = ScrapeStatus.Pending,
                CreatedAt = DateTime.Now
            };
            scrapingInformation = await _unitOfWork.ScrapingInformationRepository.InsertThenGet(scrapingInformation);
            _logger.Information($"Scraping info for ({providerCode}/{sportCode}): {JsonConvert.SerializeObject(scrapingInformation)}");
            return scrapingInformation;
        }

        public async Task<bool> UpdateScrapingInformation(ScrapingInformation scrapingInformation)
        {
            if (scrapingInformation == null)
            {
                throw new ArgumentNullException(nameof(scrapingInformation), "scrapingInformation is null");
            }

            var result = await _unitOfWork.ScrapingInformationRepository.UpdateAsync(new[] { scrapingInformation }) == 1;
            _logger.Information($"Update ScrapingInformation result: {result}");
            return result;
        }

        public async Task<List<ScrapingInformation>> GetByDateAndSportCode(DateTime fromDate, DateTime toDate, string sportCode, int scrapeType)
        {
            if (string.IsNullOrEmpty(sportCode))
            {
                throw new ArgumentException("sportCode is null");
            }

            fromDate = Helper.ToMinTime(fromDate);
            toDate = Helper.ToMaxTime(toDate);

            // If parse false => Get all
            var parsedScrapeType = Enum.IsDefined(typeof(ScrapeType), scrapeType)
                ? (ScrapeType) scrapeType
                : (ScrapeType?) null;

            var result = (await _unitOfWork.ScrapingInformationRepository.GetByDateAndSportCode(fromDate, toDate, sportCode, parsedScrapeType)).ToList();

            _logger.Information($"Scraping infos: {JsonConvert.SerializeObject(result.Select(x => x.Id))}");
            return result;
        }
    }
}
