using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Entity.Models;
using TQI.WebPortal.Service.IServices;

namespace TQI.WebPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SportController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ISportService _sportService;

        public SportController(ILogger logger, ISportService sportService)
        {
            _logger = logger;
            _sportService = sportService;
        }

        [HttpGet]
        [Route("All")]
        public async Task<ApiResult<List<Sport>>> GetAll()
        {
            var stopwatch = Stopwatch.StartNew();
            var apiResult = new ApiResult<List<Sport>>();
            try
            {
                _logger.Information("Get all sports");
                apiResult.Result = await _sportService.GetAll();
                apiResult.Succeed = true;
                _logger.Information("Get all sports complete");
            }
            catch (Exception ex)
            {
                apiResult.Succeed = false;
                apiResult.Result = null;
                apiResult.Error = ex.ToString();
                _logger.Error($"Get all sports error: {ex}");
            }
            stopwatch.Stop();
            apiResult.ExecutionTime = stopwatch.Elapsed.TotalMilliseconds;
            _logger.Information($"Execution time: {apiResult.ExecutionTime}ms");
            return apiResult;
        }

        [HttpGet]
        [Route("Provider/{sportCode}")]
        public async Task<ApiResult<List<Provider>>> GetProviders(string sportCode)
        {
            var stopwatch = Stopwatch.StartNew();
            var apiResult = new ApiResult<List<Provider>>();
            try
            {
                _logger.Information($"Get provider by sportCode: {sportCode}");
                apiResult.Result = await _sportService.GetProviders(sportCode);
                apiResult.Succeed = true;
                _logger.Information("Get provider by sportCode complete");
            }
            catch (Exception ex)
            {
                apiResult.Succeed = false;
                apiResult.Result = null;
                apiResult.Error = ex.ToString();
                _logger.Error($"Get provider by sportCode error: {ex}");
            }
            stopwatch.Stop();
            apiResult.ExecutionTime = stopwatch.Elapsed.TotalMilliseconds;
            _logger.Information($"Execution time: {apiResult.ExecutionTime}ms");
            return apiResult;
        }
    }
}
