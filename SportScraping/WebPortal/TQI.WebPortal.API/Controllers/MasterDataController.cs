using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Entity.Models;
using TQI.Infrastructure.Utility;
using TQI.WebPortal.Service.IServices;

namespace TQI.WebPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MasterDataController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMasterDataService _masterDataService;

        public MasterDataController(ILogger logger, IMasterDataService masterDataService)
        {
            _logger = logger;
            _masterDataService = masterDataService;
        }

        [HttpPost]
        [Route("Match/InsertUpdate")]
        public async Task<ApiResult<bool>> InsertUpdateMatches([FromBody] List<Match> matches)
        {
            var stopwatch = Stopwatch.StartNew();
            var apiResult = new ApiResult<bool>();
            try
            {
                _logger.Information("Start insert update matches");
                apiResult.Result = await _masterDataService.InsertUpdateMatches(matches);
                apiResult.Succeed = true;
                _logger.Information("Insert update matches complete");
            }
            catch (Exception ex)
            {
                apiResult.Succeed = false;
                apiResult.Result = false;
                apiResult.Error = ex.ToString();
                _logger.Error($"Insert update matches error: {ex}");
            }
            stopwatch.Stop();
            apiResult.ExecutionTime = stopwatch.Elapsed.TotalMilliseconds;
            _logger.Information($"Execution time: {apiResult.ExecutionTime}ms");
            return apiResult;
        }

        [HttpPost]
        [Route("Player/InsertUpdate")]
        public async Task<ApiResult<bool>> InsertUpdatePlayers([FromBody] List<Player> players)
        {
            var stopwatch = Stopwatch.StartNew();
            var apiResult = new ApiResult<bool>();
            try
            {
                _logger.Information("Start insert update players");
                apiResult.Result = await _masterDataService.InsertUpdatePlayers(players);
                apiResult.Succeed = true;
                _logger.Information("Insert update players complete");
            }
            catch (Exception ex)
            {
                apiResult.Succeed = false;
                apiResult.Result = false;
                apiResult.Error = ex.ToString();
                _logger.Error($"Insert update players error: {ex}");
            }
            stopwatch.Stop();
            apiResult.ExecutionTime = stopwatch.Elapsed.TotalMilliseconds;
            _logger.Information($"Execution time: {apiResult.ExecutionTime}ms");
            return apiResult;
        }

        [HttpGet]
        [Route("Team/{sportCode}")]
        public async Task<ApiResult<List<Team>>> GetTeams(string sportCode)
        {
            var stopwatch = Stopwatch.StartNew();
            var apiResult = new ApiResult<List<Team>>();
            try
            {
                _logger.Information($"Get teams for SportCode: {sportCode}");
                apiResult.Result = await _masterDataService.GetTeams(sportCode);
                apiResult.Succeed = true;
                _logger.Information("Get teams complete");
            }
            catch (Exception ex)
            {
                apiResult.Succeed = false;
                apiResult.Result = null;
                apiResult.Error = ex.ToString();
                _logger.Error($"Get teams error: {ex}");
            }
            stopwatch.Stop();
            apiResult.ExecutionTime = stopwatch.Elapsed.TotalMilliseconds;
            _logger.Information($"Execution time: {apiResult.ExecutionTime}ms");
            return apiResult;
        }

        [HttpGet]
        [Route("Match/{sportCode}/{fromDate}/{toDate}")]
        public async Task<ApiResult<List<Match>>> GetMatches(string sportCode, DateTime fromDate, DateTime toDate)
        {
            var stopwatch = Stopwatch.StartNew();
            var apiResult = new ApiResult<List<Match>>();
            try
            {
                _logger.Information($"Get matches for SportCode: {sportCode} - {Helper.GetDate(fromDate)} -> {Helper.GetDate(toDate)}");
                apiResult.Result = await _masterDataService.GetMatches(sportCode, fromDate, toDate);
                apiResult.Succeed = true;
                _logger.Information("Get matches complete");
            }
            catch (Exception ex)
            {
                apiResult.Succeed = false;
                apiResult.Result = null;
                apiResult.Error = ex.ToString();
                _logger.Error($"Get matches error: {ex}");
            }
            stopwatch.Stop();
            apiResult.ExecutionTime = stopwatch.Elapsed.TotalMilliseconds;
            _logger.Information($"Execution time: {apiResult.ExecutionTime}ms");
            return apiResult;
        }

        [HttpGet]
        [Route("Match/Full/{sportCode}/{fromDate}/{toDate}")]
        public async Task<ApiResult<List<Match>>> GetFullMatches(string sportCode, DateTime fromDate, DateTime toDate)
        {
            var stopwatch = Stopwatch.StartNew();
            var apiResult = new ApiResult<List<Match>>();
            try
            {
                _logger.Information($"Get full matches for SportCode: {sportCode} - {Helper.GetDate(fromDate)} -> {Helper.GetDate(toDate)}");
                apiResult.Result = await _masterDataService.GetFullMatches(sportCode, fromDate, toDate);
                apiResult.Succeed = true;
                _logger.Information("Get full matches complete");
            }
            catch (Exception ex)
            {
                apiResult.Succeed = false;
                apiResult.Result = null;
                apiResult.Error = ex.ToString();
                _logger.Error($"Get full matches error: {ex}");
            }
            stopwatch.Stop();
            apiResult.ExecutionTime = stopwatch.Elapsed.TotalMilliseconds;
            _logger.Information($"Execution time: {apiResult.ExecutionTime}ms");
            return apiResult;
        }
    }
}
