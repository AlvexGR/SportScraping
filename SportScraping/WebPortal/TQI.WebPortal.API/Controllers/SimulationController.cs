using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Entity.Database;
using TQI.Infrastructure.Entity.Database.Helpers;
using TQI.Infrastructure.Entity.Models;
using TQI.Infrastructure.Utility;
using TQI.WebPortal.Service.IServices;

namespace TQI.WebPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimulationController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ISimulationService _simulationService;

        public SimulationController(ILogger logger, ISimulationService simulationService)
        {
            _logger = logger;
            _simulationService = simulationService;
        }

        [HttpGet]
        [Route("TestApi")]
        public string Get()
        {
            _logger.Information("Test API success");
            return "Hello World";
        }

        [HttpGet]
        [Route("GetBooleanResult")]
        public async Task<ApiResult<bool>> GetBooleanResult(bool resultToBe, int awaitTime)
        {
            var result = new ApiResult<bool>();
            _logger.Information($"GetBooleanResult(): resultToBe {resultToBe} - awaitTime {awaitTime}");
            if (!resultToBe)
            {
                result.Error = "Something went wrong";
                _logger.Error("GetBooleanResult(): Something went wrong");
            }

            result.Succeed = resultToBe;
            result.Result = resultToBe;
            _logger.Information("GetBooleanResult(): Wait 1st time");
            await Task.Delay(awaitTime);
            _logger.Information("GetBooleanResult(): Wait 2nd time");
            await Task.Delay(awaitTime);
            _logger.Information("GetBooleanResult(): Wait 3rd time");
            await Task.Delay(awaitTime);
            _logger.Information("GetBooleanResult(): Done execution");
            return result;
        }

        [HttpGet]
        [Route("TestNamingComparison")]
        public bool TestNamingComparison(string source, string toCompare)
        {
            return source.CompareName(toCompare);
        }

        [HttpGet]
        [Route("TestDbToStringConverter")]
        public bool TestDbToStringConverter()
        {
            var table = DbConverter<Team>.ToTableName();
            var columnNames = DbConverter<Team>.ToColumnNames(false);
            _logger.Information($"TestDbToStringConverter(): {table} - {columnNames}");
            return true;
        }

        [HttpPost]
        [Route("TestInsertData")]
        public async Task<bool> TestInsertData([FromBody] List<TempTableToTest> tableToTests)
        {
            return await _simulationService.InsertData(tableToTests);
        }

        [HttpPut]
        [Route("TestUpdateData")]
        public async Task<bool> TestUpdateData([FromBody] List<TempTableToTest> tableToTests)
        {
            return await _simulationService.UpdateData(tableToTests);
        }

        [HttpDelete]
        [Route("TestDeleteData")]
        public async Task<bool> TestDeleteData([FromBody] List<TempTableToTest> tableToTests)
        {
            return await _simulationService.DeleteData(tableToTests);
        }
    }
}
