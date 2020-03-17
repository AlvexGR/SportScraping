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
    public class TabPlayerOverUnder : ScrapeHandler
    {
        public TabPlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public TabPlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            const string url = "https://api.beta.tab.com.au/v1/tab-info-service/sports/Basketball/competitions/NBA/matches?jurisdiction=VIC";
            var doc = await ScrapeHelper.GetDocument(url);
            var jDoc = JsonConvert.DeserializeObject<JToken>(doc);
            var rawMatches = jDoc.SelectTokens("$.matches[*]");

            await UpdateScrapeStatus(10, "Scraping match data");
            Logger.Information("Scraping match data");
            var foundMatches = new List<Match>();
            foreach (var rawMatch in rawMatches)
            {
                var sourceId = rawMatch.SelectToken("$._links.self").ToString();
                if (string.IsNullOrEmpty(sourceId))
                {
                    Logger.Warning("Source match id is null");
                    continue;
                }

                var matchName = rawMatch.SelectToken("$.name").ToString();

                var homeTeam = ScrapeHelper.RegexMappingExpression(matchName, "(.*) v .*");
                var awayTeam = ScrapeHelper.RegexMappingExpression(matchName, ".* v (.*)");

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
            Logger.Information("Scrape metric data");
            foreach (var match in foundMatches)
            {
                doc = await ScrapeHelper.GetDocument($"{match.SourceId}");
                jDoc = JsonConvert.DeserializeObject<JToken>(doc);
                var rawMetrics = jDoc.SelectTokens("$.markets[?(@.betOptionSpectrumId =~ /(837|838|839|1789|1791)/)]").ToList();

                currentRange = Math.Min(currentRange + rangeProgress, 90);

                foreach (var rawMetric in rawMetrics)
                {
                    var spectrumId = rawMetric.SelectToken("$.betOptionSpectrumId").ToString();
                    var scoreType = spectrumId == "837" ? ScoreType.Point :
                        spectrumId == "838" ? ScoreType.Rebound :
                        spectrumId == "839" ? ScoreType.Assist :
                        spectrumId == "1789" ? ScoreType.PointReboundAssist :
                        spectrumId == "1791" ? ScoreType.ThreePoint :
                        string.Empty;

                    if (string.IsNullOrEmpty(scoreType)) continue;

                    var playerOvers = rawMetric.SelectTokens("$.propositions[?(@.name =~ /(.* Over .*)/)]").ToList();
                    var playerUnders = rawMetric.SelectTokens("$.propositions[?(@.name =~ /(.* Under .*)/)]").ToList();

                    var playerNames = new HashSet<string>();
                    foreach (var playerName in playerOvers.Select(item => item.SelectToken("name").ToString()).Select(itemName => ScrapeHelper.RegexMappingExpression(itemName, "(.*)(?:Over|Under)")))
                    {
                        playerNames.Add(playerName);
                    }

                    foreach (var playerName in playerUnders.Select(item => item.SelectToken("name").ToString()).Select(itemName => ScrapeHelper.RegexMappingExpression(itemName, "(.*)(?:Over|Under)")))
                    {
                        playerNames.Add(playerName);
                    }

                    foreach (var playerName in playerNames)
                    {
                        var player = ScrapeHelper.FindPlayerInMatch(playerName, match);
                        if (player == null)
                        {
                            Logger.Warning($"Cannot find any player {playerName} in match {match.Id}");
                            continue;
                        }
                        var overOutcomeItem = playerOvers.FirstOrDefault(x => x.SelectToken("$.name").ToString().Contains(playerName));
                        var overName = overOutcomeItem?.SelectToken("name").ToString() ?? string.Empty;
                        var overLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(overName, @".* (\d*.\d*) .*"));
                        var over = ScrapeHelper.ConvertMetric(overOutcomeItem?.SelectToken("returnWin").ToString() ?? string.Empty);

                        var underOutcomeItem = playerUnders.FirstOrDefault(x => x.SelectToken("$.name").ToString().Contains(playerName));
                        var underName = underOutcomeItem?.SelectToken("name").ToString() ?? string.Empty;
                        var underLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(underName, @".* (\d*.\d*) .*"));
                        var under = ScrapeHelper.ConvertMetric(underOutcomeItem?.SelectToken("returnWin").ToString() ?? string.Empty);

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
                    }

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
