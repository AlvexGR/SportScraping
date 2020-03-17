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
    public class BorgataonlinePlayerOverUnder : ScrapeHandler
    {
        public BorgataonlinePlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public BorgataonlinePlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            const string url = "https://cds-api.borgataonline.com/bettingoffer/fixtures?x-bwin-accessid=ZTJhZDliYjgtNTdmOC00Njk0LWIxZmItODI3YzhjZGQ5NmIx&lang=en-US&country=VN&userCountry=VN&streamProviders=unas%2Cperform%2Cimgdge&fixtureTypes=Standard&state=Latest&skip=0&take=50&offerMapping=Filtered&offerCategories=Gridable&fixtureCategories=Gridable,NonGridable,Other&sortBy=Tags&sportIds=7&regionIds=9&competitionIds=6004";
            var doc = await ScrapeHelper.GetDocument(url);
            var jDoc = JsonConvert.DeserializeObject<JToken>(doc);
            var rawMatches = jDoc.SelectTokens("$.fixtures[*]");

            await UpdateScrapeStatus(10, "Scraping match data");
            var foundMatches = new List<Match>();
            foreach (var rawMatch in rawMatches)
            {
                var sourceId = rawMatch.SelectToken("$.id").ToString();
                if (string.IsNullOrEmpty(sourceId))
                {
                    Logger.Warning("Source match id is null");
                    continue;
                }
                var participants = rawMatch.SelectTokens("$.participants[*]").ToList();
                var homeTeam = participants.Last().SelectToken("$.name.value").ToString();
                var awayTeam = participants.First().SelectToken("$.name.value").ToString();

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
                var metricUrl = $"https://cds-api.borgataonline.com/bettingoffer/fixture-view?x-bwin-accessid=ZTJhZDliYjgtNTdmOC00Njk0LWIxZmItODI3YzhjZGQ5NmIx&lang=en-US&country=VN&userCountry=VN&streamProviders=unas%2Cperform%2Cimgdge&offerMapping=All&scoreboardMode=Full&fixtureIds={match.SourceId}";
                doc = await ScrapeHelper.GetDocument(metricUrl);
                jDoc = JsonConvert.DeserializeObject<JToken>(doc);

                currentRange = Math.Min(currentRange + rangeProgress, 90);

                var rawMetrics = jDoc.SelectTokens("$.fixture.games[*]").ToList();
                foreach (var rawMetric in rawMetrics)
                {
                    var nameData = rawMetric.SelectToken("$.name.value").ToString();
                    var scoreType =
                        nameData.Contains("How many points") ? ScoreType.Point :
                        nameData.Contains("How many assists") ? ScoreType.Assist :
                        nameData.Contains("How many rebounds") ? ScoreType.Rebound :
                        string.Empty;
                    if (string.IsNullOrEmpty(scoreType)) continue;

                    var playerName = ScrapeHelper.RegexMappingExpression(nameData, @".* will (.*) \(");
                    var player = ScrapeHelper.FindPlayerInMatch(playerName, match);
                    if (player == null)
                    {
                        Logger.Warning($"Cannot find any player {playerName} in match {match.Id}");
                        continue;
                    }

                    var overData = rawMetric.SelectToken("$.results[0]");
                    var over = ScrapeHelper.ConvertMetric(overData.SelectToken("$.odds").ToString());
                    var overLineData = overData.SelectToken("$.name.value").ToString();
                    var overLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(overLineData, "Over (.*)"));

                    var underData = rawMetric.SelectToken("$.results[0]");
                    var under = ScrapeHelper.ConvertMetric(underData.SelectToken("$.odds").ToString());
                    var underLineData = underData.SelectToken("$.name.value").ToString();
                    var underLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(underLineData, "Over (.*)"));

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
