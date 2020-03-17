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
    public class KambiBePlayerOverUnder : ScrapeHandler
    {
        public KambiBePlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public KambiBePlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            const string url = "https://eu-offering.kambicdn.org/offering/v2018/888/listView/basketball.json?lang=en_GB&market=ZZ&client_id=2&channel_id=1&ncid=1572482733163&useCombined=true";
            var doc = await ScrapeHelper.GetDocument(url);
            var jDoc = JsonConvert.DeserializeObject<JToken>(doc);
            var rawMatches = jDoc.SelectTokens("$.events[?(@.event.group == 'NBA')]");

            await UpdateScrapeStatus(10, "Scraping match data");
            var foundMatches = new List<Match>();
            foreach (var rawMatch in rawMatches)
            {
                var sourceId = rawMatch.SelectToken("$.event.id").ToString();
                if (string.IsNullOrEmpty(sourceId))
                {
                    Logger.Warning("Source match id is null");
                    continue;
                }

                var homeTeam = rawMatch.SelectToken("$.event.homeName").ToString();
                var awayTeam = rawMatch.SelectToken("$.event.awayName").ToString();

                var match = ScrapeHelper.FindMatchByHomeAndAwayTeam(TodayMatches, homeTeam, awayTeam);
                if (match == null)
                {
                    Logger.Warning($"Cannot find match with Home team: {homeTeam} and Away team: {awayTeam}");
                    continue;
                }

                match.SourceId = sourceId;
                foundMatches.Add(match);
            }
            await UpdateScrapeStatus(20, "Scrape match data complete");

            await UpdateScrapeStatus(20, "Scraping metric data");
            var rangeProgress = foundMatches.Count != 0 ? 90 / foundMatches.Count : 0;
            var currentRange = 20;
            foreach (var match in foundMatches)
            {
                var metricUrl = $"https://eu-offering.kambicdn.org/offering/v2018/888/betoffer/event/{match.SourceId}.json?lang=en_GB&market=ZZ&client_id=2&channel_id=1&ncid=1005710980&includeParticipants=true";
                doc = await ScrapeHelper.GetDocument(metricUrl);
                jDoc = JsonConvert.DeserializeObject<JToken>(doc);

                currentRange = Math.Min(currentRange + rangeProgress, 90);

                var rawMetrics = jDoc.SelectTokens("$.betOffers[*]").ToList();
                foreach (var rawMetric in rawMetrics)
                {
                    var labelData = rawMetric.SelectToken("$.criterion.label").ToString();
                    var scoreType = labelData.Contains("Points scored by the player") ? ScoreType.Point :
                        labelData.Contains("Rebounds by the player") ? ScoreType.Rebound :
                        labelData.Contains("Assists by the player") ? ScoreType.Assist :
                        labelData.Contains("Points, rebounds & assists by the player") ? ScoreType.PointReboundAssist :
                        labelData.Contains("3-point field goals made by the player") ? ScoreType.ThreePoint :
                        string.Empty;

                    if (string.IsNullOrEmpty(scoreType)) continue;

                    var overData = rawMetric.SelectToken("$.outcomes[?(@.label =~ /(Over.*)/)]");
                    var underData = rawMetric.SelectToken("$.outcomes[?(@.label =~ /(Under.*)/)]");

                    var playerData = overData.SelectToken("$.participant").ToString();
                    var playerSplits = playerData.Split(',');
                    var rvPlayer = playerSplits.Reverse().ToList();
                    var playerName = string.Join(" ", rvPlayer);

                    var player = ScrapeHelper.FindPlayerInMatch(playerName, match);
                    if (player == null)
                    {
                        Logger.Warning($"Cannot find any player {playerName} in match {match.Id}");
                        continue;
                    }

                    var overLineData = overData.SelectToken("$.label").ToString();
                    var overLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(overLineData, @"Over.* (\d*.\d*)"));
                    var overOdd = ScrapeHelper.ConvertMetric(overData.SelectToken("$.odds").ToString());
                    var over = overOdd / 1000;

                    var underLineData = underData.SelectToken("$.label").ToString();
                    var underLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(underLineData, @"Under.* (\d*.\d*)"));
                    var underOdd = ScrapeHelper.ConvertMetric(underData.SelectToken("$.odds").ToString());
                    var under = underOdd / 1000;

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
            await UpdateScrapeStatus(90, "Scrape metric data complete");
        }
    }
}
