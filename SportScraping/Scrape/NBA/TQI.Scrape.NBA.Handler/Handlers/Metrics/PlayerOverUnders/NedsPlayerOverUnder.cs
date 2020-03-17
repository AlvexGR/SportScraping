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
    public class NedsPlayerOverUnder : ScrapeHandler
    {
        public NedsPlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public NedsPlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            const string url = "https://api.neds.com.au/v2/sport/event-request?category_ids=%5B%223c34d075-dc14-436d-bfc4-9272a49c2b39%22%5D";
            var doc = await ScrapeHelper.GetDocument(new Uri(url));
            var jDoc = JsonConvert.DeserializeObject<JToken>(doc);
            var rawMatches = jDoc.SelectToken("$.events").ToList();

            var foundMatches = new List<Match>();

            await UpdateScrapeStatus(10, "Scraping match data");
            Logger.Information("Scraping match data");
            foreach (var matchContent in rawMatches.Select(rawMatch => rawMatch.Children().FirstOrDefault()))
            {
                if (matchContent == null)
                {
                    Logger.Warning("Match content is null");
                    continue;
                }

                var sourceMatchId = matchContent.SelectToken("$.id").ToString();
                if (string.IsNullOrEmpty(sourceMatchId))
                {
                    Logger.Warning("Source match id is null");
                    continue;
                }

                var matchName = matchContent.SelectToken("$.name").ToString();
                var competitionName = matchContent.SelectToken("$.competition.name").ToString();

                if (!matchName.Contains(" V ") || !competitionName.Contains("NBA")) continue;

                var homeTeam = ScrapeHelper.RegexMappingExpression(matchName, "(.*) V");
                var awayTeam = ScrapeHelper.RegexMappingExpression(matchName, "V (.*)");

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

            Logger.Information("Scraping metric data");
            await UpdateScrapeStatus(20, "Scraping metric data");
            foreach (var match in foundMatches)
            {
                var metricUrl = $"https://api.neds.com.au/v2/sport/event-card?id={match.SourceId}";
                doc = await ScrapeHelper.GetDocument(new Uri(metricUrl));
                jDoc = JsonConvert.DeserializeObject<JToken>(doc);

                var entrants = jDoc.SelectToken("$.entrants").ToList();
                var prices = jDoc.SelectToken("$.prices").ToList();
                ProcessMetric(match, entrants, prices, ScoreType.Point, "Points");
                ProcessMetric(match, entrants, prices, ScoreType.Rebound, "Rebounds");
                ProcessMetric(match, entrants, prices, ScoreType.Assist, "Assists");
                ProcessMetric(match, entrants, prices, ScoreType.PointReboundAssist, "PRA");
                ProcessMetric(match, entrants, prices, ScoreType.PointRebound, "PR");
                ProcessMetric(match, entrants, prices, ScoreType.PointAssist, "PA");
                ProcessMetric(match, entrants, prices, ScoreType.ReboundAssist, "RA");

                var newProgress = GetScrapingInformation().Progress;
                newProgress = Math.Min(newProgress + 90 / foundMatches.Count, 90);
                await UpdateScrapeStatus(newProgress, null);
            }
            Logger.Information("Scrape metric data complete");
            await UpdateScrapeStatus(90, "Scrape metric data complete");
        }

        private void ProcessMetric(Match match, IReadOnlyCollection<JToken> entrants,
            IReadOnlyCollection<JToken> prices, string scoreType, string actualScoreType)
        {
            var overMetrics = entrants.Where(x =>
            {
                var name = x.Children().FirstOrDefault()?.SelectToken("$.name").ToString();

                if (scoreType == "PR" || scoreType == "RA")
                {
                    return name != null && name.Contains(actualScoreType) && name.Contains("Over") && !name.Contains("PRA");
                }
                return name != null && name.Contains(actualScoreType) && name.Contains("Over");
            }).ToList();

            var underMetrics = entrants.Where(x =>
            {
                var name = x.Children().FirstOrDefault()?.SelectToken("$.name").ToString();
                if (scoreType == "PR" || scoreType == "RA")
                {
                    return name != null && name.Contains(actualScoreType) && name.Contains("Under") && !name.Contains("PRA");
                }
                return name != null && name.Contains(actualScoreType) && name.Contains("Under");
            }).ToList();

            var playerNames = new HashSet<string>();

            foreach (var content in overMetrics
                .Select(overMetric => overMetric
                                          .Children()
                                          .FirstOrDefault()?
                                          .SelectToken("$.name").ToString() ?? string.Empty))
            {
                playerNames.Add(ScrapeHelper.RegexMappingExpression(content, "(.*) Over"));
            }

            foreach (var content in underMetrics
                .Select(underMetric => underMetric.Children()
                                           .FirstOrDefault()?
                                           .SelectToken("$.name").ToString() ?? string.Empty))
            {
                playerNames.Add(ScrapeHelper.RegexMappingExpression(content, "(.*) Under"));
            }

            foreach (var playerName in playerNames)
            {
                var player = ScrapeHelper.FindPlayerInMatch(playerName, match);
                if (player == null)
                {
                    Logger.Warning($"Cannot find any player {playerName} in match {match.Id}");
                    continue;
                }

                var overContent = overMetrics.FirstOrDefault(x =>
                        x.Children().FirstOrDefault()?.SelectToken("$.name").ToString().Contains(playerName) ?? false)
                    ?.Children().FirstOrDefault();

                var underContent = underMetrics.FirstOrDefault(x =>
                        x.Children().FirstOrDefault()?.SelectToken("$.name").ToString().Contains(playerName) ?? false)
                    ?.Children().FirstOrDefault();

                var overLine =
                    ScrapeHelper.ConvertMetric(ScrapeHelper
                        .RegexMappingExpression(overContent?.SelectToken("$.name").ToString(), "Over (.*) .*"));

                var underLine =
                    ScrapeHelper.ConvertMetric(ScrapeHelper
                        .RegexMappingExpression(underContent?.SelectToken("$.name").ToString(), "Under (.*) .*"));

                var priceOverId = overContent?.SelectToken("$.id").ToString() ?? string.Empty;
                var priceUnderId = underContent?.SelectToken("$.id").ToString() ?? string.Empty;

                var priceOverData = prices.FirstOrDefault(x => x.ToString().Contains(priceOverId));
                var priceOverContent = priceOverData?.Children().FirstOrDefault();
                var numeratorOver = ScrapeHelper.ConvertMetric(priceOverContent?.SelectToken("$.odds.numerator").ToString());
                var denominatorOver = ScrapeHelper.ConvertMetric(priceOverContent?.SelectToken("$.odds.denominator").ToString());
                var over = numeratorOver != null && denominatorOver != null ? numeratorOver / denominatorOver + 1 : null;

                var priceUnderData = prices.FirstOrDefault(x => x.ToString().Contains(priceUnderId));
                var priceUnderContent = priceUnderData?.Children().FirstOrDefault();
                var numeratorUnder = ScrapeHelper.ConvertMetric(priceUnderContent.SelectToken("$.odds.numerator").ToString());
                var denominatorUnder = ScrapeHelper.ConvertMetric(priceUnderContent.SelectToken("$.odds.denominator").ToString());
                var under = numeratorUnder != null && denominatorUnder != null ? numeratorUnder / denominatorUnder + 1 : null;

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
        }
    }
}
