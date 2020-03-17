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
    public class TopSportPlayerOverUnder : ScrapeHandler
    {
        private List<PlayerOverUnder> _rawData;
        public TopSportPlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public TopSportPlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            const string url = "https://betbuilder.digitalsportstech.com/api/latestGames?leagueId=123&sb=topsport&status=1";
            var rawDoc = await ScrapeHelper.GetDocument(url);
            var jDoc = JsonConvert.DeserializeObject<JToken>(rawDoc);

            var rawMatches = jDoc.SelectTokens("$.data[*]");
            var foundMatches = new List<Match>();

            await UpdateScrapeStatus(10, "Scraping match data");
            Logger.Information("Scraping match data");
            foreach (var rawMatch in rawMatches)
            {
                var sourceMatchId = rawMatch.SelectToken("$.id").ToString();
                if (string.IsNullOrEmpty(sourceMatchId))
                {
                    Logger.Warning("Source Id is null");
                    continue;
                }

                var homeTeam = rawMatch.SelectToken("$.homeTeam.title").ToString();
                var awayTeam = rawMatch.SelectToken("$.visitingTeam.title").ToString();

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
            Logger.Information("Scraping metric data");
            _rawData = new List<PlayerOverUnder>();
            foreach (var match in foundMatches)
            {
                var metricUrl = $"https://betbuilder.digitalsportstech.com/api/feed?betType=in,18,19&gameId={match.SourceId}&limit=9999&sb=topsport&tz=7";
                rawDoc = await ScrapeHelper.GetDocument(metricUrl);
                jDoc = JsonConvert.DeserializeObject<JToken>(rawDoc);
                var rawMetrics = jDoc.SelectTokens("$.data[*]").ToList();

                currentRange = Math.Min(currentRange + rangeProgress, 80);

                foreach (var rawMetric in rawMetrics)
                {
                    var statistic = rawMetric.SelectToken("$.statistic.title").ToString();
                    var scoreType =
                        statistic.Contains("Pts + Reb + Ast") ? ScoreType.PointReboundAssist :
                        statistic.Contains("Total Rebounds") ? ScoreType.Rebound :
                        statistic.Contains("Assists") ? ScoreType.Assist :
                        statistic.Contains("Points") ? ScoreType.Point :
                        statistic.Contains("Three Point") ? ScoreType.ThreePoint :
                        string.Empty;
                    if (string.IsNullOrEmpty(scoreType)) continue;

                    var playerName = rawMetric.SelectToken("$.player1.name").ToString();
                    var player = ScrapeHelper.FindPlayerInMatch(playerName, match);
                    if (player == null)
                    {
                        Logger.Warning($"Cannot find any player {playerName} in match {match.Id}");
                        continue;
                    }

                    var betType = rawMetric.SelectToken("$.betType").ToString();

                    double? over = 0, overLine = 0, under = 0, underLine = 0;
                    switch (betType)
                    {
                        case "18":
                            over = ScrapeHelper.ConvertMetric(rawMetric.SelectToken("$.markets[0].odds").ToString());
                            overLine = ScrapeHelper.ConvertMetric(rawMetric.SelectToken("$.markets[0].value").ToString());
                            break;
                        case "19":
                            under = ScrapeHelper.ConvertMetric(rawMetric.SelectToken("$.markets[0].odds").ToString());
                            underLine = ScrapeHelper.ConvertMetric(rawMetric.SelectToken("$.markets[0].value").ToString());
                            break;
                    }

                    var metric = new PlayerOverUnder
                    {
                        MatchId = match.Id,
                        Over = over,
                        OverLine = overLine,
                        Under = under,
                        UnderLine = underLine,
                        PlayerId = player.Id,
                        Player = player,
                        ScoreType = scoreType,
                        ScrapingInformationId = GetScrapingInformation().Id,
                        CreatedAt = DateTime.Now
                    };

                    _rawData.Add(metric);

                    var newProgress = GetScrapingInformation().Progress;
                    newProgress = Math.Min(newProgress + currentRange / rawMetrics.Count, currentRange);
                    await UpdateScrapeStatus(newProgress, null);
                }
                await UpdateScrapeStatus(currentRange, null);
            }
            Logger.Information("Scrape metric data complete");

            MergeData();
            Logger.Information("Merge data complete");

            await UpdateScrapeStatus(90, "Scrape metric data complete");
        }

        private void MergeData()
        {
            var groupByScoreType = _rawData.GroupBy(x => new
            {
                x.Match,
                x.PlayerId,
                x.ScoreType
            });

            foreach (var group in groupByScoreType)
            {
                var metric = group.First();
                foreach (var data in group)
                {
                    if (data.Over != null && data.Over.Value.CompareTo(0) != 0)
                    {
                        metric.Over = data.Over;
                    }
                    if (data.OverLine != null && data.OverLine.Value.CompareTo(0) != 0)
                    {
                        metric.OverLine = data.OverLine;
                    }
                    if (data.Under != null && data.Under.Value.CompareTo(0) != 0)
                    {
                        metric.Under = data.Under;
                    }
                    if (data.UnderLine != null && data.UnderLine.Value.CompareTo(0) != 0)
                    {
                        metric.UnderLine = data.UnderLine;
                    }
                }

                PlayerUnderOvers.Add(metric);

                // Printing purpose pattern only. Apply for all ScrapeHandlers
                var player = metric.Player;
                var scoreType = metric.ScoreType;
                var over = metric.Over;
                var overLine = metric.OverLine;
                var under = metric.Under;
                var underLine = metric.UnderLine;
                Logger.Information($"{player.Name}: {scoreType} - {over} {overLine} | {under} {underLine}");
            }
        }
    }
}
