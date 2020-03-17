using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using TQI.Infrastructure.Entity.Models;
using TQI.Infrastructure.Entity.Models.Metrics;
using TQI.Infrastructure.Scrape.Handler;
using TQI.Infrastructure.Utility;

namespace TQI.Scrape.NBA.Handler.Handlers.Metrics.PlayerHeadToHeads
{
    public class TabPlayerHeadToHead : ScrapeHandler
    {
        public TabPlayerHeadToHead(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
            logger = null;
        }

        public TabPlayerHeadToHead()
        {
        }

        protected override async Task ScrapeData()
        {
            const string url = "https://api.beta.tab.com.au/v1/tab-info-service/sports/Basketball/competitions/NBA/matches?jurisdiction=VIC";
            var doc = await ScrapeHelper.GetDocument(url);
            var jDoc = JsonConvert.DeserializeObject<JToken>(doc);

            var rawMatches = jDoc.SelectTokens("$.matches[*]");
            var foundMatches = new List<Match>();

            await UpdateScrapeStatus(10, "Scraping match data");
            Logger.Information("Scraping match data");
            foreach (var rawMatch in rawMatches)
            {
                var sourceMatchId = rawMatch.SelectToken("$._links.self").ToString();
                if (string.IsNullOrEmpty(sourceMatchId))
                {
                    Logger.Warning("Source Id is null");
                    continue;
                }

                var homeTeam = rawMatch.SelectTokens("$.contestants[?(@.position == 'HOME')].name")?.FirstOrDefault()?.ToString();
                var awayTeam = rawMatch.SelectTokens("$.contestants[?(@.position == 'AWAY')].name")?.FirstOrDefault()?.ToString();

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
            foreach (var match in foundMatches)
            {
                doc = await ScrapeHelper.GetDocument(match.SourceId);
                jDoc = JsonConvert.DeserializeObject<JToken>(doc);
                var rawMetrics = jDoc.SelectTokens("$.markets[?(@.betOptionSpectrumId =~ /(605)/)]").ToList();

                currentRange = Math.Min(currentRange + rangeProgress, 90);

                foreach (var rawMetric in rawMetrics)
                {
                    var propositionPlayerA = rawMetric.SelectToken("$.propositions[0]");
                    var propositionTie = rawMetric.SelectToken("$.propositions[1]");
                    var propositionPlayerB = rawMetric.SelectToken("$.propositions[2]");

                    var playerNameA = ScrapeHelper.RegexMappingExpression(propositionPlayerA.SelectToken("$.name").ToString(), @"(.*)\(");
                    var playerNameB = ScrapeHelper.RegexMappingExpression(propositionPlayerB.SelectToken("$.name").ToString(), @"(.*)\(");
                    var playerA = ScrapeHelper.FindPlayerInMatch(playerNameA, match);
                    if (playerA == null)
                    {
                        Logger.Warning($"Cannot find any playerA {playerNameA} in match {match.Id}");
                        continue;
                    }

                    var playerB = ScrapeHelper.FindPlayerInMatch(playerNameB, match);
                    if (playerB == null)
                    {
                        Logger.Warning($"Cannot find any playerB {playerNameB} in match {match.Id}");
                        continue;
                    }

                    var priceA = ScrapeHelper.ConvertMetric(propositionPlayerA.SelectToken("$.returnWin").ToString());
                    var tiePrice = ScrapeHelper.ConvertMetric(propositionTie.SelectToken("$.returnWin").ToString());
                    var priceB = ScrapeHelper.ConvertMetric(propositionPlayerB.SelectToken("$.returnWin").ToString());

                    Logger.Information($"{playerA.Name} vs {playerB.Name}: {priceA} vs {priceB} | Tie: {tiePrice}");
                    var metric = new PlayerHeadToHead
                    {
                        PlayerAId = playerA.Id,
                        PlayerBId = playerB.Id,
                        IsTieIncluded = tiePrice != null,
                        PlayerAPrice = priceA,
                        PlayerBPrice = priceB,
                        ScrapingInformationId = GetScrapingInformation().Id,
                        TiePrice = tiePrice,
                        MatchId = match.Id,
                        CreatedAt = DateTime.Now
                    };

                    PlayerHeadToHeads.Add(metric);

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
