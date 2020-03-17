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
    public class PalmerbetPlayerOverUnder : ScrapeHandler
    {
        public PalmerbetPlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public PalmerbetPlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            const string url = "https://fixture.palmerbet.online/fixtures/sports/1c2eeb3a-6bab-4ac2-b434-165cc350180f/matches?pageSize=25";
            var rawResult = await ScrapeHelper.GetDocument(url);
            var matchObj = JsonConvert.DeserializeObject<JToken>(rawResult);

            var rawMatches = matchObj.SelectTokens("$.matches[*]");
            var foundMatches = new List<Match>();
            await UpdateScrapeStatus(10, "Scraping match data");
            foreach (var rawMatch in rawMatches)
            {
                var sourceMatchId = rawMatch.SelectToken("$.eventId").ToString();
                if (string.IsNullOrEmpty(sourceMatchId))
                {
                    Logger.Warning("Source Id is null");
                    continue;
                }

                var homeTeam = rawMatch.SelectToken("$.homeTeam.title").ToString();
                var awayTeam = rawMatch.SelectToken("$.awayTeam.title").ToString();
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

            var rawMetricsTasks = foundMatches
                .Select(match => $"https://fixture.palmerbet.online/fixtures/sports/matches/{match.SourceId}/markets?pageSize=1000")
                .Select(metricUrl => ScrapeHelper.GetDocument(metricUrl))
                .ToList();

            var rawMetrics = await Task.WhenAll(rawMetricsTasks);
            await UpdateScrapeStatus(20, "Scraping metric data");

            var rangeProgress = rawMetrics.Length != 0 ? 90 / rawMetrics.Length : 0;
            var currentRange = 20;

            Logger.Information($"Total raw metrics count {rawMetrics.Length}");
            var totalMetrics = 0;
            try
            {
                for (var i = 0; i < rawMetrics.Length; i++)
                {
                    var rawMetric = rawMetrics[i];
                    var metricObj = JsonConvert.DeserializeObject<JToken>(rawMetric);
                    var metricMarkets = metricObj.SelectTokens("$.markets[?(@.title=~ /(.* - .*)/)]").ToList();

                    currentRange = Math.Min(currentRange + rangeProgress, 90);

                    foreach (var metricMarket in metricMarkets)
                    {
                        var title = metricMarket.SelectToken("$.title").ToString();
                    
                        var scoreType =
                            title.Contains("Points") ? ScoreType.Point :
                            title.Contains("Rebounds") ? ScoreType.Rebound :
                            title.Contains("Assists") ? ScoreType.Assist :
                            title.Contains("Made Threes") ? ScoreType.ThreePoint :
                            title.Contains("Pts + Ast") ? ScoreType.PointAssist :
                            title.Contains("Pts + Reb + Ast") ? ScoreType.PointReboundAssist :
                            title.Contains("Pts + Reb") ? ScoreType.PointRebound :
                            title.Contains("Reb + Ast") ? ScoreType.ReboundAssist :
                            string.Empty;

                        if (string.IsNullOrEmpty(scoreType)) continue;
                        var playerName = title.Split('-')[0].Trim();
                        var player = ScrapeHelper.FindPlayerInMatch(playerName, foundMatches[i]);
                        if (player == null)
                        {
                            Logger.Warning($"Cannot find any player {playerName} in match {foundMatches[i].Id}");
                            continue;
                        }

                        totalMetrics++;

                        var playerId = metricMarket.SelectToken("$.id").ToString();
                        var priceUrl = $"https://fixture.palmerbet.online/fixtures/sports/markets/{playerId}";
                        var priceDoc = await ScrapeHelper.GetDocument(priceUrl, 3000);

                        var priceObj = JsonConvert.DeserializeObject<JToken>(priceDoc);

                        if (priceObj == null)
                        {
                            Logger.Warning($"priceObj is null for player: {playerId}");
                            continue;
                        }

                        var outcomeItems = priceObj.SelectTokens("$.market.outcomes[*]");
                        double? over = 0, overLine = 0, under = 0, underLine = 0;
                        foreach (var outcomeItem in outcomeItems)
                        {
                            var titleItem = outcomeItem.SelectToken("$.title").ToString();

                            if (titleItem.Contains("Over"))
                            {
                                var rawOver = ScrapeHelper.RegexMappingExpression(titleItem, "(?:Over) (.*)");
                                var rawPrice = outcomeItem.SelectTokens("$.prices[*]").FirstOrDefault()
                                    .SelectToken("$.priceSnapshot.current").ToString();

                                overLine = ScrapeHelper.ConvertMetric(rawOver);
                                over = ScrapeHelper.ConvertMetric(rawPrice);
                            }
                            else if (titleItem.Contains("Under"))
                            {
                                var rawUnder = ScrapeHelper.RegexMappingExpression(titleItem, "(?:Under) (.*)");
                                var rawPrice = outcomeItem.SelectTokens("$.prices[*]").FirstOrDefault()
                                    .SelectToken("$.priceSnapshot.current").ToString();

                                underLine = ScrapeHelper.ConvertMetric(rawUnder);
                                under = ScrapeHelper.ConvertMetric(rawPrice);
                            }
                        }

                        Logger.Information($"{player.Name}: {scoreType} - {over} {overLine} | {under} {underLine}");

                        var metric = new PlayerOverUnder
                        {
                            PlayerId = player.Id,
                            ScoreType = scoreType,
                            ScrapingInformationId = GetScrapingInformation().Id,
                            MatchId = foundMatches[i].Id,
                            Over = over,
                            OverLine = overLine,
                            Under = under,
                            UnderLine = underLine,
                            CreatedAt = DateTime.Now
                        };

                        PlayerUnderOvers.Add(metric);

                        var newProgress = GetScrapingInformation().Progress;
                        newProgress = Math.Min(newProgress + currentRange / metricMarkets.Count, currentRange);
                        await UpdateScrapeStatus(newProgress, null);
                    }

                    await UpdateScrapeStatus(currentRange, null);
                }
                await UpdateScrapeStatus(90, "Scrape metric data complete");

                Logger.Information($"Total metric count: {totalMetrics}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                throw;
            }
        }
    }
}
