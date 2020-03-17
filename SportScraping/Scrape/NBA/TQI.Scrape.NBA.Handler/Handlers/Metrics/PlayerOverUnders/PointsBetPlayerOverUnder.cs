using System;
using System.Collections.Generic;
using System.Linq;
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
    public class PointsBetPlayerOverUnder : ScrapeHandler
    {
        public PointsBetPlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public PointsBetPlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            const string url = "https://api.pointsbet.com/api/v2/competitions/7176/events/featured?includeLive=true";
            var doc = await ScrapeHelper.GetDocument(url);
            var jDoc = JsonConvert.DeserializeObject<JToken>(doc);
            var rawMatches = jDoc.SelectTokens("$.events[*]");
            var foundMatches = new List<Match>();

            await UpdateScrapeStatus(10, "Scraping match data");
            Logger.Information("Scraping match data");
            foreach (var rawMatch in rawMatches)
            {
                var sourceId = rawMatch.SelectToken("$.key").ToString();
                if (string.IsNullOrEmpty(sourceId))
                {
                    Logger.Warning("Source Id is null");
                    continue;
                }
                var homeTeam = rawMatch.SelectToken("$.homeTeam").ToString();
                var awayTeam = rawMatch.SelectToken("$.awayTeam").ToString();
                var match = ScrapeHelper.FindMatchByHomeAndAwayTeam(TodayMatches, homeTeam, awayTeam);

                if (match == null)
                {
                    Logger.Warning($"Cannot find match with Home team: {homeTeam} and Away team: {awayTeam}");
                    continue;
                }

                match.SourceId = sourceId;
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
                var metricUrl = $"https://api.pointsbet.com/api/v2/events/{match.SourceId}";
                doc = await ScrapeHelper.GetDocument(metricUrl);
                jDoc = JsonConvert.DeserializeObject<JToken>(doc);

                currentRange = Math.Min(currentRange + rangeProgress, 90);

                var rawMetrics = jDoc
                    .SelectTokens(
                        @"$.fixedOddsMarkets[?(@.name =~ /(Player Points Over\/Under .*)/ || @.name =~ /(Player Rebounds Over\/Under .*)/ || @.name =~ /(Player Assists Over\/Under .*)/)]")
                    .ToList();
                foreach (var rawMetric in rawMetrics)
                {
                    var metricName = rawMetric.SelectToken("$.name").ToString();
                    var scoreType =
                        metricName.Contains("Player Points") ? ScoreType.Point :
                        metricName.Contains("Player Assists") ? ScoreType.Assist :
                        metricName.Contains("Player Rebounds") ? ScoreType.Rebound :
                        metricName.Contains("Player Pts + Rebs + Asts") ? ScoreType.PointReboundAssist :
                        string.Empty;

                    if (string.IsNullOrEmpty(scoreType)) continue;

                    var outcomeOvers = rawMetric.SelectTokens("$.outcomes[?(@.name =~ /(.* Over .*)/)]").ToList();
                    var outcomeUnders = rawMetric.SelectTokens("$.outcomes[?(@.name =~ /(.* Under .*)/)]").ToList();

                    var playerNames = new HashSet<string>();

                    foreach (var name in outcomeOvers.Select(outcome => outcome.SelectToken("name").ToString()))
                    {
                        playerNames.Add(ScrapeHelper.RegexMappingExpression(name, "(.*)(?:Over|Under)"));
                    }

                    foreach (var name in outcomeUnders.Select(outcome => outcome.SelectToken("name").ToString()))
                    {
                        playerNames.Add(ScrapeHelper.RegexMappingExpression(name, "(.*)(?:Over|Under)"));
                    }

                    foreach (var playerName in playerNames)
                    {
                        var player = ScrapeHelper.FindPlayerInMatch(playerName, match);
                        if (player == null)
                        {
                            Logger.Warning($"Cannot find any player {playerName} in match {match.Id}");
                            continue;
                        }

                        var outcomeOver = outcomeOvers.FirstOrDefault(x =>
                            x.SelectToken("$.name").ToString().Contains(playerName));
                        var outcomeUnder = outcomeUnders.FirstOrDefault(x =>
                            x.SelectToken("$.name").ToString().Contains(playerName));

                        var rawOver = outcomeOver?.SelectToken("price").ToString();
                        var rawOverLine =
                            ScrapeHelper.RegexMappingExpression(outcomeOver?.SelectToken("$.name").ToString(),
                                "(?:Over) (.*)");

                        var rawUnder = outcomeUnder?.SelectToken("price").ToString();
                        var rawUnderLine =
                            ScrapeHelper.RegexMappingExpression(outcomeUnder?.SelectToken("$.name").ToString(),
                                "(?:Under) (.*)");

                        Logger.Information($"{player.Name}: {scoreType} - {rawOver} {rawOverLine} | {rawUnder} {rawUnderLine}");

                        var metric = new PlayerOverUnder
                        {
                            MatchId = match.Id,
                            Over = ScrapeHelper.ConvertMetric(rawOver),
                            OverLine = ScrapeHelper.ConvertMetric(rawOverLine),
                            Under = ScrapeHelper.ConvertMetric(rawUnder),
                            UnderLine = ScrapeHelper.ConvertMetric(rawUnderLine),
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
            }
            Logger.Information("Scrape metric data complete");
            await UpdateScrapeStatus(90, "Scrape metric data complete");
        }
    }
}
