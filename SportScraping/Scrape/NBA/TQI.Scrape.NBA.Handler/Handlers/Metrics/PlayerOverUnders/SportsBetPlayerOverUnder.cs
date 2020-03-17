using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Entity.Models;
using TQI.Infrastructure.Entity.Models.Metrics;
using TQI.Infrastructure.Scrape.Handler;
using TQI.Infrastructure.Utility;

namespace TQI.Scrape.NBA.Handler.Handlers.Metrics.PlayerOverUnders
{
    public class SportsBetPlayerOverUnder : ScrapeHandler
    {
        public SportsBetPlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public SportsBetPlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            const string url = "https://www.sportsbet.com.au/apigw/sportsbook-sports/Sportsbook/Sports/Competitions/6927/Events";
            var rawDoc = await ScrapeHelper.GetDocument(url);
            var jDoc = JsonConvert.DeserializeObject<JToken>(rawDoc);

            var rawMatches = jDoc.SelectTokens("$.[*]");
            var foundMatches = new List<Match>();

            await UpdateScrapeStatus(10, "Scraping match data");
            Logger.Information("Scraping match data");
            foreach (var rawMatch in rawMatches)
            {
                if (rawMatch.SelectToken("$.competitionName").ToString() != "NBA Matches") continue;

                var sourceMatchId = rawMatch.SelectToken("$.id").ToString();
                if (string.IsNullOrEmpty(sourceMatchId))
                {
                    Logger.Warning("Source Id is null");
                    continue;
                }

                var matchName = rawMatch.SelectToken("$.name").ToString();
                var teams = matchName.Split(new[] { " At " }, StringSplitOptions.None);
                var homeTeam = teams[1];
                var awayTeam = teams[0];

                var match = ScrapeHelper.FindMatchByHomeAndAwayTeam(TodayMatches, homeTeam, awayTeam);
                if (match == null)
                {
                    Logger.Warning($"Cannot find match with Home team: {homeTeam} and Away team: {awayTeam}");
                    continue;
                }

                match.SourceId = sourceMatchId;
                foundMatches.Add(match);
            }
            Logger.Information("Scrape match data complete");
            await UpdateScrapeStatus(20, "Scrape match data complete");

            await UpdateScrapeStatus(20, "Scraping metric data");
            var rangeProgress = foundMatches.Count != 0 ? 90 / foundMatches.Count : 0;
            var currentRange = 20;
            Logger.Information("Scrape metric data");
            foreach (var match in foundMatches)
            {
                var pointsUrl = $"https://www.sportsbet.com.au/apigw/sportsbook-sports/Sportsbook/Sports/Events/{match.SourceId}/MarketGroupings/567/Markets";
                var reboundsUrl = $"https://www.sportsbet.com.au/apigw/sportsbook-sports/Sportsbook/Sports/Events/{match.SourceId}/MarketGroupings/568/Markets";
                var assistsUrl = $"https://www.sportsbet.com.au/apigw/sportsbook-sports/Sportsbook/Sports/Events/{match.SourceId}/MarketGroupings/569/Markets";
                var combinationUrl = $"https://www.sportsbet.com.au/apigw/sportsbook-sports/Sportsbook/Sports/Events/{match.SourceId}/MarketGroupings/570/Markets";
                
                currentRange = Math.Min(currentRange + rangeProgress, 90);

                var docTasks = new List<Task<string>>
                {
                    ScrapeHelper.GetDocument(pointsUrl),
                    ScrapeHelper.GetDocument(reboundsUrl),
                    ScrapeHelper.GetDocument(assistsUrl),
                    ScrapeHelper.GetDocument(combinationUrl)
                };

                var results = await Task.WhenAll(docTasks);
                var rawMetrics = new List<JToken>();
                foreach (var result in results)
                {
                    rawMetrics.AddRange(JsonConvert.DeserializeObject<JToken>(result).SelectTokens("$.[*]"));
                }

                foreach (var rawMetric in rawMetrics)
                {
                    var metricName = rawMetric.SelectToken("$.name").ToString();
                    var scoreType =
                        metricName.Contains("Pts + Reb + Ast") ? ScoreType.PointReboundAssist :
                        metricName.Contains("Points") ? ScoreType.Point :
                        metricName.Contains("Assists") ? ScoreType.Assist :
                        metricName.Contains("Made Threes") ? ScoreType.ThreePoint :
                        metricName.Contains("Rebounds") ? ScoreType.Rebound :
                        metricName.Contains("Pts + Reb") ? ScoreType.PointRebound :
                        metricName.Contains("Pts + Ast") ? ScoreType.PointAssist :
                        metricName.Contains("Reb + Ast") ? ScoreType.ReboundAssist :
                        string.Empty;

                    if (string.IsNullOrEmpty(scoreType)) continue;

                    var playerName = metricName.Split('-')[0];
                    var player = ScrapeHelper.FindPlayerInMatch(playerName, match);
                    if (player == null)
                    {
                        Logger.Warning($"Cannot find any player {playerName} in match {match.Id}");
                        continue;
                    }

                    var selection = rawMetric.SelectToken("$.selections[0]");
                    var selectionName = selection.SelectToken("$.name").ToString();
                    double? over = 0, overLine = 0, under = 0, underLine = 0;
                    if (selectionName.Contains("Over"))
                    {
                        over = ScrapeHelper.ConvertMetric(selection.SelectToken("$.price.winPrice")?.ToString());
                        overLine = ScrapeHelper.ConvertMetric(selection.SelectToken("$.unformattedHandicap")?.ToString());
                    }
                    else if (selectionName.Contains("Under"))
                    {
                        under = ScrapeHelper.ConvertMetric(selection.SelectToken("$.price.winPrice")?.ToString());
                        underLine = ScrapeHelper.ConvertMetric(selection.SelectToken("$.unformattedHandicap")?.ToString());
                    }

                    Logger.Information($"{player.Name}: {scoreType} - {over} {overLine} | {under} {underLine}");

                    var metric = new PlayerOverUnder
                    {
                        PlayerId = player.Id,
                        ScoreType = scoreType,
                        ScrapingInformationId = GetScrapingInformation().Id,
                        MatchId = match.Id,
                        Over = over,
                        OverLine = overLine,
                        Under = under,
                        UnderLine = underLine,
                        CreatedAt = DateTime.Now
                    };

                    PlayerUnderOvers.Add(metric);

                    var newProgress = GetScrapingInformation().Progress;
                    newProgress = Math.Min(newProgress + currentRange / rawMetrics.Count, currentRange);
                    await UpdateScrapeStatus(newProgress, null);
                }
                await UpdateScrapeStatus(currentRange, null);
            }
            Logger.Information("Scrape metric data complete");
            await UpdateScrapeStatus(90, "Scrape metric data complete");
        }
    }
}
