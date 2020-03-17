using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Entity.Models.Metrics;
using TQI.WebPortal.Service.IServices;

namespace TQI.WebPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetricDataController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMetricDataService _metricDataService;

        public MetricDataController(ILogger logger, IMetricDataService metricDataService)
        {
            _logger = logger;
            _metricDataService = metricDataService;
        }

        [HttpPost]
        [Route("PlayerUnderOver/Insert")]
        public async Task<ApiResult<bool>> InsertPlayerUnderOvers([FromBody] List<PlayerOverUnder> playerUnderOvers)
        {
            var stopwatch = Stopwatch.StartNew();
            var apiResult = new ApiResult<bool>();
            try
            {
                _logger.Information($"Insert player under over metric: {JsonConvert.SerializeObject(playerUnderOvers)}");
                apiResult.Result = await _metricDataService.InsertPlayerUnderOvers(playerUnderOvers);
                apiResult.Succeed = true;
                _logger.Information("Insert player under over metric complete");
            }
            catch (Exception ex)
            {
                apiResult.Succeed = false;
                apiResult.Result = false;
                apiResult.Error = ex.ToString();
                _logger.Error($"Insert player under over metric error: {ex}");
            }
            stopwatch.Stop();
            apiResult.ExecutionTime = stopwatch.Elapsed.TotalMilliseconds;
            _logger.Information($"Execution time: {apiResult.ExecutionTime}ms");
            return apiResult;
        }

        [HttpPost]
        [Route("PlayerHeadToHead/Insert")]
        public async Task<ApiResult<bool>> InsertPlayerHeadToHeads([FromBody] List<PlayerHeadToHead> playerHeadToHeads)
        {
            var stopwatch = Stopwatch.StartNew();
            var apiResult = new ApiResult<bool>();
            try
            {
                _logger.Information($"Insert player head to head metric: {JsonConvert.SerializeObject(playerHeadToHeads)}");
                apiResult.Result = await _metricDataService.InsertPlayerHeadToHeads(playerHeadToHeads);
                apiResult.Succeed = true;
                _logger.Information("Insert player head to head metric complete");
            }
            catch (Exception ex)
            {
                apiResult.Succeed = false;
                apiResult.Result = false;
                apiResult.Error = ex.ToString();
                _logger.Error($"Insert player head to head metric error: {ex}");
            }
            stopwatch.Stop();
            apiResult.ExecutionTime = stopwatch.Elapsed.TotalMilliseconds;
            _logger.Information($"Execution time: {apiResult.ExecutionTime}ms");
            return apiResult;
        }

        [HttpGet]
        [Route("Export/{countryCode}/{sportCode}/{dateTime}")]
        public async Task<IActionResult> ExportMetricData(string countryCode, string sportCode, DateTime dateTime)
        {
            var stopwatch = Stopwatch.StartNew();
            byte[] data;
            try
            {
                _logger.Information($"Export metric data: {countryCode}, {sportCode}, {dateTime}");
                data = await _metricDataService.ExportMetricData(countryCode, sportCode, dateTime);
                _logger.Information("Export metric data complete");
            }
            catch (Exception ex)
            {
                _logger.Error($"Export metric data error: {ex}");
                return NotFound();
            }
            stopwatch.Stop();
            _logger.Information($"Execution time: {stopwatch.Elapsed.TotalMilliseconds}ms");

            if (data != null && data.Length != 0) return File(data, "application/excel");

            _logger.Warning("No data to export");
            return NotFound();
        }
    }
}
