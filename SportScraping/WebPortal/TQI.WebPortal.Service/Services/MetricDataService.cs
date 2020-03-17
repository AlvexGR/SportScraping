using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using Serilog;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Entity.Excels;
using TQI.Infrastructure.Entity.Models;
using TQI.Infrastructure.Entity.Models.Metrics;
using TQI.WebPortal.Service.IServices;
using TQI.WebPortal.Service.UnitOfWork;

namespace TQI.WebPortal.Service.Services
{
    public class MetricDataService : IMetricDataService
    {
        private readonly ILogger _logger;
        private readonly IWebPortalUnitOfWork _unitOfWork;

        public MetricDataService(ILogger logger, IWebPortalUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> InsertPlayerUnderOvers(List<PlayerOverUnder> playerUnderOvers)
        {
            if (playerUnderOvers == null || playerUnderOvers.Count == 0)
            {
                throw new ArgumentException("playerUnderOvers is null or empty");
            }

            _logger.Information($"Insert player over under for ScrapeInfo Id: {playerUnderOvers.First().ScrapingInformationId}");

            var result = await _unitOfWork.PlayerOverUnderRepository.InsertAsync(playerUnderOvers);
            _logger.Information($"Insert result: {result}/{playerUnderOvers.Count}");
            return result == playerUnderOvers.Count;
        }

        public async Task<bool> InsertPlayerHeadToHeads(List<PlayerHeadToHead> playerHeadToHeads)
        {
            if (playerHeadToHeads == null || playerHeadToHeads.Count == 0)
            {
                throw new ArgumentException("playerHeadToHeads is null or empty");
            }

            _logger.Information($"Insert player over under for ScrapeInfo Id: {playerHeadToHeads.First().ScrapingInformationId}");

            var result = await _unitOfWork.PlayerHeadToHeadRepository.InsertAsync(playerHeadToHeads);
            _logger.Information($"Insert result: {result}/{playerHeadToHeads.Count}");
            return result == playerHeadToHeads.Count;
        }

        public async Task<byte[]> ExportMetricData(string countryCode, string sportCode, DateTime dateTime)
        {
            if (string.IsNullOrEmpty(countryCode)) throw new ArgumentNullException(nameof(countryCode));
            if (string.IsNullOrEmpty(sportCode)) throw new ArgumentNullException(nameof(sportCode));

            var overUnderData = new List<PlayerOverUnder>();
            var headToHeadData = new List<PlayerHeadToHead>();

            _logger.Information("Get player over under data");
            try
            {
                overUnderData =
                    (await _unitOfWork.PlayerOverUnderRepository.GetByCountryAndSportCode(countryCode, sportCode,
                        dateTime)).ToList();
            }
            catch (Exception ex)
            {
                // continue to process
                _logger.Error($"PlayerOverUnder export error: {ex}");
            }
            _logger.Information("Get player head to head data");
            try
            {
                headToHeadData = (await _unitOfWork.PlayerHeadToHeadRepository.GetByCountryAndSportCode(countryCode, sportCode, dateTime)).ToList();
            }
            catch (Exception ex)
            {
                // continue to process
                _logger.Error($"PlayerHeadToHead export error: {ex}");
            }

            if (overUnderData.Count == 0)
            {
                _logger.Warning("OverUnderData is empty");
            }

            if (headToHeadData.Count == 0)
            {
                _logger.Warning("HeadToHeadData is empty");
            }

            var providers =
                (await _unitOfWork.ProviderRepository.GetProvidersBySportCodeAndCountryCode(sportCode, countryCode))
                .ToList();
            overUnderData = overUnderData.OrderBy(x => x.Match.StartTime).ToList();
            var sheets = new List<MetricExcelSheet>();
            sheets.AddRange(ConvertDataForOverUnder(overUnderData, providers.Where(x => x.ScrapeType == ScrapeType.PlayerOverUnder)));
            sheets.Add(ConvertDataForHeadToHead(headToHeadData));
            return ConvertToExcelFile(sheets);
        }

        private static IEnumerable<MetricExcelSheet> ConvertDataForOverUnder(IReadOnlyCollection<PlayerOverUnder> playerOverUnders, IEnumerable<Provider> providers)
        {
            var scoreTypes = new List<string>
            {
                ScoreType.Point,
                ScoreType.Assist,
                ScoreType.PointAssist,
                ScoreType.PointRebound,
                ScoreType.PointReboundAssist,
                ScoreType.Rebound,
                ScoreType.ThreePoint
            };

            var values = new List<string>
            {
                "Over",
                "LineOver",
                "Under",
                "LineUnder"
            };

            var providersList = providers.ToList();
            var result = new List<MetricExcelSheet>();

            foreach (var scoreType in scoreTypes)
            {
                var groupData = playerOverUnders
                    .Where(x => x.ScoreType == scoreType)
                    .GroupBy(x => new { x.MatchId, x.PlayerId, x.ScrapingInformation.ProviderId })
                    .Select(x => x
                        .OrderByDescending(y => y.CreatedAt)
                        .FirstOrDefault())
                    .ToList();
                
                foreach (var value in values)
                {
                    var sheet = new MetricExcelSheet
                    {
                        Name = $"{scoreType}_{value}",
                        ColumnNames = new List<string>(),
                        Data = new List<List<string>>()
                    };

                    sheet.ColumnNames.Add("GameCode");
                    sheet.ColumnNames.Add("PlayerId");
                    sheet.ColumnNames.Add("PlayerName");

                    sheet.ColumnNames.AddRange(providersList.Select(x => x.Name));

                    var dataDict = new Dictionary<(string, string, string), List<string>>();

                    foreach (var item in groupData.Where(item => item != null))
                    {
                        if (!dataDict.TryGetValue((item.Match.GameCode, item.Player.SourceId, item.Player.Name),
                            out var curData))
                        {
                            curData = providersList.Select(x => string.Empty).ToList();

                            dataDict.Add((item.Match.GameCode,
                                item.Player.SourceId,
                                item.Player.Name), curData);
                        }

                        for (var i = 0; i < providersList.Count; i++)
                        {
                            var provider = providersList[i];
                            curData[i] = value switch
                            {
                                "Over" => (item.ScrapingInformation.ProviderId == provider.Id && item.Over != null &&
                                           item.Over.Value.CompareTo(0) != 0
                                    ? item.Over.ToString()
                                    : string.Empty),
                                "LineOver" => (item.ScrapingInformation.ProviderId == provider.Id &&
                                               item.OverLine != null && item.OverLine.Value.CompareTo(0) != 0
                                    ? item.OverLine.ToString()
                                    : string.Empty),
                                "Under" => (item.ScrapingInformation.ProviderId == provider.Id && item.Under != null &&
                                            item.Under.Value.CompareTo(0) != 0
                                    ? item.Under.ToString()
                                    : string.Empty),
                                "LineUnder" => (item.ScrapingInformation.ProviderId == provider.Id &&
                                                item.UnderLine != null && item.UnderLine.Value.CompareTo(0) != 0
                                    ? item.UnderLine.ToString()
                                    : string.Empty),
                                _ => curData[i] // Default case
                            };
                        }
                    }

                    

                    foreach (var ((gameCode, playerSourceId, playerName), list) in dataDict)
                    {
                        var hasValue = list.Aggregate(false,
                            (current, metric) => current | !string.IsNullOrEmpty(metric));

                        if (!hasValue) continue;
                        var data = new List<string> {gameCode, playerSourceId, playerName};
                        data.AddRange(list);
                        sheet.Data.Add(data);
                    }

                    result.Add(sheet);
                }
            }

            return result;
        }

        private MetricExcelSheet ConvertDataForHeadToHead(IEnumerable<PlayerHeadToHead> playerHeadToHeads)
        {
            var sheet = new MetricExcelSheet
            {
                Name = "H2H_P",
                ColumnNames = new List<string>
                {
                    "SOURCE", "PLAYER_A", "PLAYER_B", "TIE_INCLUDED",
                    "PLAYER_A_PRICE", "PLAYER_B_PRICE", "TIE_PRICE"
                },
                Data = new List<List<string>>()
            };

            var groupData = playerHeadToHeads.GroupBy(x => x.Id);
            foreach (var group in groupData)
            {
                if (group.Count() != 2)
                {
                    _logger.Error($"This group has {group.Count()} items instead of 2");
                    continue;
                }

                var data = new List<string>
                {
                    group.First().ScrapingInformation.Provider.Name
                };
                foreach (var headToHead in group)
                {
                    if (headToHead.PlayerA != null)
                    {
                        data.Add(headToHead.PlayerA.Name);
                    }
                    else if (headToHead.PlayerB != null)
                    {
                        data.Add(headToHead.PlayerB.Name);
                    }
                    else
                    {
                        _logger.Error($"Both players are null for {headToHead.Id}");
                    }
                }

                data.Add(group.First().IsTieIncluded ? "YES" : "NO");
                data.Add(group.First().PlayerAPrice.ToString());
                data.Add(group.First().PlayerBPrice.ToString());
                data.Add(group.First().TiePrice.ToString());
                sheet.Data.Add(data);
            }

            return sheet;
        }

        private static byte[] ConvertToExcelFile(IEnumerable<MetricExcelSheet> excelSheets)
        {
            var package = new ExcelPackage();
            foreach (var excelSheet in excelSheets)
            {
                var worksheet = package.Workbook.Worksheets.Add(excelSheet.Name);
                const int headerIndex = 1; // row
                worksheet.Row(headerIndex).Style.Font.Bold = true;
                for (var i = 1; i <= excelSheet.ColumnNames.Count; i++)
                {
                    worksheet.Cells[headerIndex, i].Value = excelSheet.ColumnNames[i - 1];
                }

                var recordIndex = 2; // row
                foreach (var record in excelSheet.Data)
                {
                    var colIndex = 1;
                    foreach (var item in record)
                    {
                        worksheet.Cells[recordIndex, colIndex++].Value = item;
                    }

                    recordIndex++;
                }

                for (var i = 1; i < excelSheet.ColumnNames.Count; i++)
                {
                    worksheet.Column(i).AutoFit();
                }
            }

            return package.GetAsByteArray();
        }
    }
}
