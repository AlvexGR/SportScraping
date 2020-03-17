using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Chrome;
using Serilog;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Entity.Models;
using TQI.Infrastructure.Entity.Models.Metrics;
using TQI.Infrastructure.Scrape.Handler;
using TQI.Infrastructure.Utility;

namespace TQI.Scrape.NBA.Handler.Handlers.Metrics.PlayerOverUnders
{
    public class BetEasyPlayerOverUnder : ScrapeHandler
    {
        public BetEasyPlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public BetEasyPlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            var chromeDriver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            try
            {
                const string url = "https://beteasy.com.au/api/sports/sports?filters%5BEventTypes%5D=107&filters%5BMasterCategoryID%5D=37";
                chromeDriver.Navigate().GoToUrl(url);
                await Task.Delay(10000);

                var pageSource = ScrapeHelper.RegexMappingExpression(chromeDriver.PageSource, "({.*})");
                var jDocument = JsonConvert.DeserializeObject<JToken>(pageSource);
                var rawMatches = jDocument.SelectTokens("$.result.sports.events[*]");

                await UpdateScrapeStatus(10, "Scraping match data");

                Logger.Information("Scraping match data");
                var foundMatches = new List<Match>();
                foreach (var rawMatch in rawMatches)
                {
                    var sourceMatchId = rawMatch.SelectToken("$.MasterEventID").ToString();
                    if (string.IsNullOrEmpty(sourceMatchId))
                    {
                        Logger.Warning("Source match id is null");
                        continue;
                    }

                    var eventName = rawMatch.SelectToken("$.MasterEventName").ToString();
                    if (string.IsNullOrEmpty(eventName))
                    {
                        Logger.Warning("Even name is null");
                        continue;
                    }

                    var homeTeam = eventName.Split('@')[1];
                    var awayTeam = eventName.Split('@')[0];

                    var match = ScrapeHelper.FindMatchByHomeAndAwayTeam(TodayMatches, homeTeam, awayTeam);
                    if (match == null)
                    {
                        Logger.Warning($"Cannot find match with Home team: {homeTeam} and Away team: {awayTeam}");
                        continue;
                    }

                    match.SourceId = sourceMatchId;
                    foundMatches.Add(match);
                }
                await UpdateScrapeStatus(20, "Scrape match data complete");
                Logger.Information("Scrape matches done");

                await UpdateScrapeStatus(20, "Scrape single metrics data");
                var rangeProgress = foundMatches.Count != 0 ? 55 / foundMatches.Count : 0;
                var currentRange = 20;
                Logger.Information("Scrape single metrics data");
                foreach (var match in foundMatches)
                {
                    var singleMetricUrl =
                        $"https://beteasy.com.au/api/sports/event-group/?id={match.SourceId}&ecGroupOrderByIds%5B%5D=18";

                    chromeDriver.Navigate().GoToUrl(singleMetricUrl);
                    await Task.Delay(5000);

                    currentRange = Math.Min(currentRange + rangeProgress, 55);

                    var metricPageSource = ScrapeHelper.RegexMappingExpression(chromeDriver.PageSource, "({.*})");
                    var jRawMetrics = JsonConvert.DeserializeObject<JToken>(metricPageSource);
                    var rawMetrics = jRawMetrics.SelectTokens("$.result.*.BettingType[*]").ToList();

                    foreach (var rawMetric in rawMetrics)
                    {
                        var eventName = rawMetric.SelectToken("$.EventName").ToString();
                        var scoreType =
                            eventName.Contains("Points") ? ScoreType.Point :
                            eventName.Contains("Rebounds") ? ScoreType.Rebound :
                            eventName.Contains("Assists") ? ScoreType.Assist :
                            string.Empty;

                        if (string.IsNullOrEmpty(scoreType)) continue;

                        var playerName = ScrapeHelper.RegexMappingExpression(eventName, "(.*) (Points|Rebounds|Assists)");
                        var player = ScrapeHelper.FindPlayerInMatch(playerName, match);
                        if (player == null)
                        {
                            Logger.Warning($"Cannot find any player {playerName} in match {match.Id}");
                            continue;
                        }

                        var outcomes = rawMetric.SelectTokens("$.Outcomes[*]");
                        double? over = 0, overLine = 0, under = 0, underLine = 0;
                        foreach (var outcome in outcomes)
                        {
                            var outcomeName = outcome.SelectToken("$.OutcomeName").ToString();
                            if (outcomeName.Contains("Over"))
                            {
                                var rawOver = ScrapeHelper.RegexMappingExpression(outcomeName, "Over (.*)");
                                var rawPrice = outcome.SelectToken("$.BetTypes[0]").SelectToken("$.Price").ToString();

                                overLine = ScrapeHelper.ConvertMetric(rawOver);
                                over = ScrapeHelper.ConvertMetric(rawPrice);
                            }
                            else if (outcomeName.Contains("Under"))
                            {
                                var rawUnder = ScrapeHelper.RegexMappingExpression(outcomeName, "Under (.*)");
                                var rawPrice = outcome.SelectToken("$.BetTypes[0]").SelectToken("$.Price").ToString();

                                underLine = ScrapeHelper.ConvertMetric(rawUnder);
                                under = ScrapeHelper.ConvertMetric(rawPrice);
                            }
                        }
                        Logger.Information($"{player.Name}: {scoreType} - {over} {overLine} | {under} {underLine}");

                        var metric = new PlayerOverUnder
                        {
                            MatchId = match.Id,
                            Over = over,
                            OverLine = overLine,
                            Under = under,
                            UnderLine = underLine,
                            PlayerId = player.Id,
                            ScoreType = scoreType,
                            ScrapingInformationId = GetScrapingInformation().Id,
                            CreatedAt = DateTime.Now
                        };

                        PlayerUnderOvers.Add(metric);

                        var newProgress = GetScrapingInformation().Progress;
                        newProgress = Math.Min(newProgress + currentRange / rawMetrics.Count, currentRange);
                        await UpdateScrapeStatus(newProgress, null);
                    }
                    await UpdateScrapeStatus(currentRange, null);
                }
                Logger.Information("Scrape single metrics data complete");
                await UpdateScrapeStatus(55, "Scrape single metrics data complete");

                Logger.Information("Scrape combination metrics data");

                rangeProgress = foundMatches.Count != 0 ? 90 / foundMatches.Count : 0;
                currentRange = 55;
                await UpdateScrapeStatus(55, "Scrape combination metrics data");
                foreach (var match in foundMatches)
                {
                    var combinationMetricUrl =
                        $"https://beteasy.com.au/api/sports/event-group/?id={match.SourceId}&ecGroupOrderByIds%5B%5D=24";

                    chromeDriver.Navigate().GoToUrl(combinationMetricUrl);
                    await Task.Delay(5000);

                    currentRange = Math.Min(currentRange + rangeProgress, 90);

                    var metricPageSource = ScrapeHelper.RegexMappingExpression(chromeDriver.PageSource, "({.*})");
                    var jRawMetrics = JsonConvert.DeserializeObject<JToken>(metricPageSource);
                    var rawMetrics = jRawMetrics.SelectTokens("$.result.*.BettingType[*]").ToList();

                    foreach (var rawMetric in rawMetrics)
                    {
                        var eventName = rawMetric.SelectToken("$.EventName").ToString();
                        var scoreType =
                            eventName.Contains("Points + Rebounds + Assists") ? ScoreType.PointReboundAssist :
                            eventName.Contains("Points + Rebounds") ? ScoreType.PointRebound :
                            eventName.Contains("Points + Assists") ? ScoreType.PointAssist :
                            eventName.Contains("Rebounds + Assists") ? ScoreType.ReboundAssist :
                            string.Empty;

                        if (string.IsNullOrEmpty(scoreType)) continue;
                        var outcomes = rawMetric.SelectTokens("$.Outcomes[*]").ToList();

                        Player player = null;
                        foreach (var outcomeName in outcomes.Select(outcome => outcome.SelectToken("$.OutcomeName").ToString()))
                        {
                            if (outcomeName.Contains("Over"))
                            {
                                var playerName = ScrapeHelper.RegexMappingExpression(outcomeName, "(.*) Over");
                                player = ScrapeHelper.FindPlayerInMatch(playerName, match);
                                if (player != null) continue;
                                Logger.Warning($"Cannot find any player {playerName} in match {match.Id}");
                            }
                            else if (outcomeName.Contains("Under"))
                            {
                                var playerName = ScrapeHelper.RegexMappingExpression(outcomeName, "(.*) Under");
                                player = ScrapeHelper.FindPlayerInMatch(playerName, match);
                                if (player != null) continue;
                                Logger.Warning($"Cannot find any player {playerName} in match {match.Id}");
                            }
                        }

                        if (player == null)
                        {
                            continue;
                        }

                        double? over = 0, overLine = 0, under = 0, underLine = 0;
                        foreach (var outcome in outcomes)
                        {
                            var outcomeName = outcome.SelectToken("$.OutcomeName").ToString();
                            if (outcomeName.Contains("Over"))
                            {
                                var rawOver = ScrapeHelper.RegexMappingExpression(outcomeName, "Over (.*)");
                                var rawPrice = outcome.SelectToken("$.BetTypes[0]").SelectToken("$.Price").ToString();

                                overLine = string.IsNullOrEmpty(rawOver) ? (double?)null : Convert.ToDouble(rawOver);
                                over = string.IsNullOrEmpty(rawPrice) ? (double?)null : Convert.ToDouble(rawPrice);
                            }
                            else if (outcomeName.Contains("Under"))
                            {
                                var rawUnder = ScrapeHelper.RegexMappingExpression(outcomeName, "Under (.*)");
                                var rawPrice = outcome.SelectToken("$.BetTypes[0]").SelectToken("$.Price").ToString();

                                underLine = string.IsNullOrEmpty(rawUnder) ? (double?)null : Convert.ToDouble(rawUnder);
                                under = string.IsNullOrEmpty(rawPrice) ? (double?)null : Convert.ToDouble(rawPrice);
                            }
                        }
                        Logger.Information($"{player.Name}: {scoreType} - {over} {overLine} | {under} {underLine}");

                        var metric = new PlayerOverUnder
                        {
                            MatchId = match.Id,
                            Over = over,
                            OverLine = overLine,
                            Under = under,
                            UnderLine = underLine,
                            PlayerId = player.Id,
                            ScoreType = scoreType,
                            ScrapingInformationId = GetScrapingInformation().Id,
                            CreatedAt = DateTime.Now
                        };

                        PlayerUnderOvers.Add(metric);

                        var newProgress = GetScrapingInformation().Progress;
                        newProgress = Math.Min(newProgress + currentRange / rawMetrics.Count, currentRange);
                        await UpdateScrapeStatus(newProgress, null);
                    }
                    await UpdateScrapeStatus(currentRange, null);
                }
                Logger.Information("Scrape combination metrics data complete");
                await UpdateScrapeStatus(90, "Scrape combination metrics data complete");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                chromeDriver.Quit();
                throw;
            }
            chromeDriver.Quit();
        }
    }
}
