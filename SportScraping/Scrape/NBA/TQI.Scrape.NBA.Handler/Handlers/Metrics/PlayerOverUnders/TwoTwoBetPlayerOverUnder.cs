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
using TQI.Infrastructure.Entity.Models;
using TQI.Infrastructure.Entity.Models.Metrics;
using TQI.Infrastructure.Scrape.Handler;
using TQI.Infrastructure.Utility;

namespace TQI.Scrape.NBA.Handler.Handlers.Metrics.PlayerOverUnders
{
    public class TwoTwoBetPlayerOverUnder : ScrapeHandler
    {
        public TwoTwoBetPlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public TwoTwoBetPlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            var chromeDriver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            try
            {
                const string url = "https://nodejs08.tglab.io/cache/20/en/us/11624/prematch-by-tournaments.json";
                chromeDriver.Navigate().GoToUrl(url);
                await Task.Delay(2000);

                var pageSource = ScrapeHelper.RegexMappingExpression(chromeDriver.PageSource, "({.*})");
                var jDocument = JsonConvert.DeserializeObject<JToken>(pageSource);
                var rawMatches = jDocument.SelectTokens("$.events[*]");

                var foundMatches = new List<Match>();
                foreach (var rawMatch in rawMatches)
                {
                    var sourceId = rawMatch.SelectToken("$.id").ToString();
                    if (string.IsNullOrEmpty(sourceId))
                    {
                        Logger.Warning("Source match id is null");
                        continue;
                    }

                    var homeTeam = rawMatch.SelectToken("$.teams.home").ToString();
                    var awayTeam = rawMatch.SelectToken("$.teams.away").ToString();

                    var match = ScrapeHelper.FindMatchByHomeAndAwayTeam(TodayMatches, homeTeam, awayTeam);
                    if (match == null)
                    {
                        Logger.Warning($"Cannot find match with Home team: {homeTeam} and Away team: {awayTeam}");
                        continue;
                    }

                    match.SourceId = sourceId;
                    foundMatches.Add(match);
                }

                var tempMetrics = new List<PlayerOverUnder>();
                foreach (var match in foundMatches)
                {
                    var metricUrl = $"https://nodejs08.tglab.io/cache/20/en/us/{match.SourceId}/single-pre-event.json?hidenseek=95183b1a46914705952f351b5bc156fd8e1e7b57be65d492e09186129ff70e8a1ba8";
                    chromeDriver.Navigate().GoToUrl(metricUrl);
                    await Task.Delay(2000);

                    var pageSourceMarket = ScrapeHelper.RegexMappingExpression(chromeDriver.PageSource, "({.*})");
                    var marketData = JsonConvert.DeserializeObject<JToken>(pageSourceMarket);

                    var rawMetrics = marketData.SelectTokens("$.odds.*");
                    foreach (var rawMetric in rawMetrics)
                    {
                        var teamName = rawMetric.SelectToken("$.team_name.en").ToString();
                        if (!teamName.Contains("total points")) continue;

                        var playerSplits = ScrapeHelper.RegexMappingExpression(teamName, "(.*) total").Split('.');
                        var playerName = $"{playerSplits[0]} {playerSplits[1]}";
                        var player = ScrapeHelper.FindPlayerInMatch(playerName, match);
                        if (player == null)
                        {
                            Logger.Warning($"Cannot find any player {playerName} in match {match.Id}");
                            continue;
                        }

                        var metric = tempMetrics.FirstOrDefault(x => x.PlayerId == player.Id);
                        if (teamName.Contains("OVER"))
                        {
                            if (metric != null)
                            {
                                metric.OverLine = ScrapeHelper.ConvertMetric(rawMetric.SelectToken("$.additional_value_raw").ToString());
                                metric.Over = ScrapeHelper.ConvertMetric(rawMetric.SelectToken("$.odd_value").ToString());
                            }
                            else
                            {
                                var newMetric = new PlayerOverUnder
                                {
                                    MatchId = match.Id,
                                    PlayerId = player.Id,
                                    OverLine = ScrapeHelper.ConvertMetric(rawMetric.SelectToken("$.additional_value_raw").ToString()),
                                    Over = ScrapeHelper.ConvertMetric(rawMetric.SelectToken("$.odd_value").ToString()),
                                    ScrapingInformationId = GetScrapingInformation().Id,
                                    CreatedAt = DateTime.Now
                                };
                                tempMetrics.Add(newMetric);
                            }
                        }
                        else if (teamName.Contains("UNDER"))
                        {
                            if (metric != null)
                            {
                                metric.UnderLine = ScrapeHelper.ConvertMetric(rawMetric.SelectToken("$.additional_value_raw").ToString());
                                metric.Under = ScrapeHelper.ConvertMetric(rawMetric.SelectToken("$.odd_value").ToString());
                            }
                            else
                            {
                                var newMetric = new PlayerOverUnder
                                {
                                    MatchId = match.Id,
                                    PlayerId = player.Id,
                                    UnderLine = ScrapeHelper.ConvertMetric(rawMetric.SelectToken("$.additional_value_raw").ToString()),
                                    Under = ScrapeHelper.ConvertMetric(rawMetric.SelectToken("$.odd_value").ToString()),
                                    ScrapingInformationId = GetScrapingInformation().Id,
                                    CreatedAt = DateTime.Now
                                };
                                tempMetrics.Add(newMetric);
                            }
                        }
                    }
                }

                foreach (var metric in tempMetrics)
                {
                    Logger.Information($"{metric.Player.Name}: " +
                                       $"{metric.Over} {metric.OverLine} | " +
                                       $"{metric.Under} {metric.UnderLine}");
                }

                PlayerUnderOvers.AddRange(tempMetrics);
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
