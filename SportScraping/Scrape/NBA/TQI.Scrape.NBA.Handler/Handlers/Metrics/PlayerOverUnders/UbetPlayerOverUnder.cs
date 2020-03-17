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
    public class UbetPlayerOverUnder : ScrapeHandler
    {
        public UbetPlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public UbetPlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            const string url = "https://ubet.com/api/sportsViewData/mainevents/false/6698";
            var doc = await ScrapeHelper.GetDocument(url);
            var jDoc = JsonConvert.DeserializeObject<JToken>(doc);

            var rawMatches = jDoc.SelectTokens("$.[*]");
            var foundMatches = new List<Match>();

            await UpdateScrapeStatus(10, "Scraping match data");
            Logger.Information("Scraping match data");
            foreach (var rawMatch in rawMatches)
            {
                var sourceMatchId = rawMatch.SelectToken("$.MainEventId").ToString();
                if (string.IsNullOrEmpty(sourceMatchId))
                {
                    Logger.Warning("Source Id is null");
                    continue;
                }

                var mainEventName = rawMatch.SelectToken("$.MainEventName").ToString();
                var date = Helper.GetCurrentDate("dd/M");
                var teams = mainEventName
                    .Split(new[] {date}, StringSplitOptions.None)[0]
                    .Split(new[] {" v "}, StringSplitOptions.None);

                var homeTeam = teams[0];
                var awayTeam = teams[1];

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
            var betTypes = new HashSet<string>();
            foreach (var match in foundMatches)
            {
                var metricUrl = $"https://ubet.com/api/sportsViewData/markets/false/{match.SourceId}";
                doc = await ScrapeHelper.GetDocument(metricUrl);
                jDoc = JsonConvert.DeserializeObject<JToken>(doc);
                var rawMetrics = jDoc
                    .SelectTokens("$.All[?(@.GroupId == -1)].SubEvents[?(@.LongDisplayName =~ /(.* (Ttl|Total).*)/)]")
                    .ToList();

                currentRange = Math.Min(currentRange + rangeProgress, 90);

                foreach (var rawMetric in rawMetrics)
                {
                    var betTypeShortName = rawMetric.SelectToken("$.BetTypeShortName").ToString();
                    var scoreType =
                        betTypeShortName.Contains("Total Points Scored") ? ScoreType.Point :
                        betTypeShortName.Contains("Total Pts+Rebound+Assist") ? ScoreType.PointReboundAssist :
                        string.Empty;

                    betTypes.Add(betTypeShortName);

                    if (string.IsNullOrEmpty(scoreType)) continue;
                    var longDisplayName = rawMetric.SelectToken("$.LongDisplayName").ToString();
                    var playerName = ScrapeHelper.RegexMappingExpression(longDisplayName, @"(.*)(?:Ttl|Total)");
                    var player = ScrapeHelper.FindPlayerInMatch(playerName, match);
                    if (player == null)
                    {
                        Logger.Warning($"Cannot find any player {playerName} in match {match.Id}");
                        continue;
                    }

                    double? over = 0, overLine = 0, under = 0, underLine = 0;
                    var offers = rawMetric.SelectTokens(@"$.Offers[?(@.OfferName =~ /(.* Over|Under|OV|UN .*)/)]");
                    foreach (var offer in offers)
                    {
                        var offerName = offer.SelectToken("$.OfferName").ToString();
                        if (offerName.Contains("Over") || offerName.Contains("OV"))
                        {
                            over = ScrapeHelper.ConvertMetric(offer.SelectToken("$.WinReturn").ToString());
                            overLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(offerName, @".* (\d*.\d)"));
                        }
                        else if (offerName.Contains("Under") || offerName.Contains("UN"))
                        {
                            under = ScrapeHelper.ConvertMetric(offer.SelectToken("$.WinReturn").ToString());
                            underLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(offerName, @".* (\d*.\d)"));
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
            Logger.Information($"Unique scoreTypes: {JsonConvert.SerializeObject(betTypes)}"); // notify to update
            Logger.Information("Scrape metric data complete");
            await UpdateScrapeStatus(90, "Scrape metric data complete");
        }
    }
}
