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
    public class BetVictorPlayerOverUnder : ScrapeHandler
    {
        public BetVictorPlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public BetVictorPlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            const string url = "https://www.betvictor.com/api/top_bets/601600?number_to_show=20&exclude_in_running=true&event_ids_to_exclude=&sport_ids_to_exclude=";
            var rawDoc = await ScrapeHelper.GetDocument(url);
            var jDoc = JsonConvert.DeserializeObject<JToken>(rawDoc);

            await UpdateScrapeStatus(10, "Scraping match data");
            var rawMatches = jDoc.SelectTokens("$.[?(@.meeting_description == 'NBA')]");
            var foundMatches = new List<Match>();
            foreach (var rawMatch in rawMatches)
            {
                var sourceMatchId = rawMatch.SelectToken("$.event_id").ToString();
                if (string.IsNullOrEmpty(sourceMatchId))
                {
                    Logger.Warning("Source Id is null");
                    continue;
                }

                if (foundMatches.Select(x => x.SourceId).Contains(sourceMatchId)) continue;

                var game = rawMatch.SelectToken("$.event_description").ToString();
                var clubs = game.Split('@');
                var homeTeam = clubs[1];
                var awayTeam = clubs[0];
                
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

            var rangeProgress = foundMatches.Count != 0 ? 90 / foundMatches.Count : 0;
            var currentRange = 20;
            await UpdateScrapeStatus(20, "Scraping metric data");
            foreach (var match in foundMatches)
            {
                rawDoc = await ScrapeHelper.GetDocument($"https://www.betvictor.com/bv_event_level/en-gb/1/coupons/{match.SourceId}/4787?t=1573192805056");
                var jResult = JsonConvert.DeserializeObject<JToken>(rawDoc);

                currentRange = Math.Min(currentRange + rangeProgress, 90);

                var rawMetrics = jResult.SelectTokens("$.[*].markets[*]").ToList();
                foreach (var rawMetric in rawMetrics)
                {
                    var description = rawMetric.SelectToken("$.des").ToString();
                    var scoreType =
                        description.Contains("Total combined points, assists & rebounds") ? ScoreType.PointReboundAssist :
                        description.Contains("Total points") ? ScoreType.Point :
                        description.Contains("Total assists") ? ScoreType.Assist :
                        description.Contains("Total three pointers") ? ScoreType.ThreePoint :
                        description.Contains("Total rebounds") ? ScoreType.Rebound :
                        description.Contains("Total combined points & rebounds") ? ScoreType.PointRebound :
                        description.Contains("Total combined points & assists") ? ScoreType.PointAssist :
                        description.Contains("Total combined assists & rebounds") ? ScoreType.ReboundAssist :
                        string.Empty;

                    if (string.IsNullOrEmpty(scoreType)) continue;

                    var playerName = rawMetric.SelectToken("$.opponentDescription").ToString();
                    var player = ScrapeHelper.FindPlayerInMatch(playerName, match);
                    if (player == null)
                    {
                        Logger.Warning($"Cannot find any player {playerName} in match {match.Id}");
                        continue;
                    }

                    var playerMetrics = rawMetric.SelectTokens("$.o[*]");

                    double? over = 0, overLine = 0, under = 0, underLine = 0;
                    foreach (var metricItem in playerMetrics)
                    {
                        var des = metricItem.SelectToken("$.des").ToString();
                        if (des.Contains("Over"))
                        {
                            overLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(des, "Over (.*)"));
                            over = ScrapeHelper.ConvertMetric(metricItem.SelectToken("$.pr").ToString());
                        }
                        else if (des.Contains("Under"))
                        {
                            underLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(des, "Under (.*)"));
                            under = ScrapeHelper.ConvertMetric(metricItem.SelectToken("$.pr").ToString());
                        }
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
            await UpdateScrapeStatus(90, "Scrape metric data complete");
        }
    }
}
