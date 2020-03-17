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
    public class BovadaPlayerOverUnder : ScrapeHandler
    {
        public BovadaPlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public BovadaPlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            const string url = "https://services.bovada.lv/services/sports/event/coupon/events/A/description/basketball/nba?marketFilterId=def&preMatchOnly=true&lang=en";
            var doc = await ScrapeHelper.GetDocument(url);
            var jDoc = JsonConvert.DeserializeObject<JToken>(doc);
            var rawMatches = jDoc.SelectTokens("$.[*].events[*]");

            await UpdateScrapeStatus(10, "Scraping match data");
            var foundMatches = new List<Match>();
            foreach (var rawMatch in rawMatches)
            {
                var sourceId = rawMatch.SelectToken("$.link").ToString();
                if (string.IsNullOrEmpty(sourceId))
                {
                    Logger.Warning("Source match id is null");
                    continue;
                }
                var description = rawMatch.SelectToken("$.description").ToString();
                var desSplits = description.Split('@');
                var homeTeam = desSplits[1];
                var awayTeam = desSplits[0];

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

            var rangeProgress = foundMatches.Count != 0 ? 90 / foundMatches.Count : 0;
            var currentRange = 20;
            await UpdateScrapeStatus(20, "Scraping metric data");
            foreach (var match in foundMatches)
            {
                var metricUrl = $"https://services.bovada.lv/services/sports/event/coupon/events/A/description{match.SourceId}?lang=en";
                doc = await ScrapeHelper.GetDocument(metricUrl);
                jDoc = JsonConvert.DeserializeObject<JToken>(doc);

                currentRange = Math.Min(currentRange + rangeProgress, 90);

                var rawMetrics = jDoc
                    .SelectTokens("$.[*].events[*].displayGroups[?(@.description == 'Player Props')].markets[*]")
                    .ToList();
                foreach (var rawMetric in rawMetrics)
                {
                    var description = rawMetric.SelectToken("$.description").ToString();
                    var scoreType =
                        description.Contains("Total Points, Rebounds and Assists") ? ScoreType.PointReboundAssist :
                        description.Contains("Total Points and Rebounds") ? ScoreType.PointRebound :
                        description.Contains("Total Points and Assists") ? ScoreType.PointAssist :
                        description.Contains("Total Rebounds and Assists") ? ScoreType.ReboundAssist :
                        description.Contains("Total Points") ? ScoreType.Point :
                        description.Contains("Total Rebounds") ? ScoreType.Rebound :
                        description.Contains("Total Assists") ? ScoreType.Assist :
                        string.Empty;

                    if (string.IsNullOrEmpty(scoreType)) continue;

                    var playerName = ScrapeHelper.RegexMappingExpression(description, @"- (.*) \(");
                    var player = ScrapeHelper.FindPlayerInMatch(playerName, match);
                    if (player == null)
                    {
                        Logger.Warning($"Cannot find any player {playerName} in match {match.Id}");
                        continue;
                    }

                    var outcomes = rawMetric.SelectTokens("$.outcomes[*]");
                    double? over = 0, overLine = 0, under = 0, underLine = 0;
                    foreach (var outcome in outcomes)
                    {
                        var des = outcome.SelectToken("$.description").ToString();
                        if (des.Contains("Over"))
                        {
                            overLine = ScrapeHelper.ConvertMetric(outcome.SelectToken("$.price.handicap").ToString());
                            var overPrice = ScrapeHelper.ConvertMetric(outcome.SelectToken("$.price.decimal").ToString());
                            over = overPrice != null ? Math.Round((double) overPrice, 2) : (double?) null;
                        }
                        else if (des.Contains("Under"))
                        {
                            underLine = ScrapeHelper.ConvertMetric(outcome.SelectToken("$.price.handicap").ToString());
                            var underPrice = ScrapeHelper.ConvertMetric(outcome.SelectToken("$.price.decimal").ToString());
                            under = underPrice != null ? Math.Round((double)underPrice, 2) : (double?)null;
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
            await UpdateScrapeStatus(90, "Scrape metric data complete");
        }
    }
}
