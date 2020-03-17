using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Entity.Models;
using TQI.WebPortal.Service.IServices;

namespace TQI.WebPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScrapingController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IScrapingService _scrapingService;

        public ScrapingController(ILogger logger, IScrapingService scrapingService)
        {
            _logger = logger;
            _scrapingService = scrapingService;
        }

        [HttpGet]
        [Route("{fromDate}/{toDate}/{sportCode}/{scrapeType}")]
        public async Task<ApiResult<List<ScrapingInformation>>> GetByDateAndSportCode(DateTime fromDate, DateTime toDate, string sportCode, int scrapeType)
        {
            var stopwatch = Stopwatch.StartNew();
            var apiResult = new ApiResult<List<ScrapingInformation>>();
            try
            {
                _logger.Information($"Get scraping information: {fromDate}->{toDate}/{sportCode} - {scrapeType}");
                apiResult.Result = await _scrapingService.GetByDateAndSportCode(fromDate, toDate, sportCode, scrapeType);
                apiResult.Succeed = true;
                _logger.Information("Get scraping information complete");
            }
            catch (Exception ex)
            {
                apiResult.Succeed = false;
                apiResult.Result = null;
                apiResult.Error = ex.ToString();
                _logger.Error($"Get scraping information error: {ex}");
            }
            stopwatch.Stop();
            apiResult.ExecutionTime = stopwatch.Elapsed.TotalMilliseconds;
            _logger.Information($"Execution time: {apiResult.ExecutionTime}ms");
            return apiResult;
        }

        [HttpPost]
        [Route("Init/{sportCode}/{providerCode}")]
        public async Task<ApiResult<ScrapingInformation>> InitScrapingInformation(string sportCode, string providerCode)
        {
            var stopwatch = Stopwatch.StartNew();
            var apiResult = new ApiResult<ScrapingInformation>();
            try
            {
                _logger.Information($"Init scraping information for provider: {providerCode}/{sportCode}");
                apiResult.Result = await _scrapingService.InitScrapingInformation(sportCode, providerCode);
                apiResult.Succeed = true;
                _logger.Information("Init scraping information complete");
            }
            catch (Exception ex)
            {
                apiResult.Succeed = false;
                apiResult.Result = null;
                apiResult.Error = ex.ToString();
                _logger.Error($"Init scraping information error: {ex}");
            }
            stopwatch.Stop();
            apiResult.ExecutionTime = stopwatch.Elapsed.TotalMilliseconds;
            _logger.Information($"Execution time: {apiResult.ExecutionTime}ms");
            return apiResult;
        }

        [HttpPost]
        [Route("UpdateScrapingInformation")]
        public async Task<ApiResult<bool>> UpdateProgress([FromBody] ScrapingInformation scrapingInformation)
        {
            var stopwatch = Stopwatch.StartNew();
            var apiResult = new ApiResult<bool>();
            try
            {
                _logger.Information($"Update progress for scraping information: {JsonConvert.SerializeObject(scrapingInformation)}");
                apiResult.Result = await _scrapingService.UpdateScrapingInformation(scrapingInformation);
                apiResult.Succeed = true;
                _logger.Information("Update progress complete");
            }
            catch (Exception ex)
            {
                apiResult.Succeed = false;
                apiResult.Result = false;
                apiResult.Error = ex.ToString();
                _logger.Error($"Update progress error: {ex}");
            }
            stopwatch.Stop();
            apiResult.ExecutionTime = stopwatch.Elapsed.TotalMilliseconds;
            _logger.Information($"Execution time: {apiResult.ExecutionTime}ms");
            return apiResult;
        }
    }
}
