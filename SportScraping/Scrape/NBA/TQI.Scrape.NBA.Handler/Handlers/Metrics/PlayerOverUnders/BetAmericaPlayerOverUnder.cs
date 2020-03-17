using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using Serilog;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Entity.Models.Metrics;
using TQI.Infrastructure.Scrape.Handler;
using TQI.Infrastructure.Utility;

namespace TQI.Scrape.NBA.Handler.Handlers.Metrics.PlayerOverUnders
{
    public class BetAmericaPlayerOverUnder : ScrapeHandler
    {
        public BetAmericaPlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public BetAmericaPlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            var chromeDriver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            try
            {
                const string url = "https://nj.betamerica.com/sports/basketball/nba/";
                chromeDriver.Navigate().GoToUrl(url);
                await Task.Delay(20000);

                var doc = new HtmlDocument();
                doc.LoadHtml(chromeDriver.PageSource);
                var rawMatches = doc.DocumentNode.SelectNodes("/html/body/div[4]/div/div[2]/div/section[7]/div[2]/div[2]/div/sb-comp/div[1]/div/sb-lazy-render/div[*]");

                var sourceIds = rawMatches
                    .Select(rawMatch => rawMatch.SelectSingleNode("div/a"))
                    .Select(aTag => aTag.GetAttributeValue("href", string.Empty))
                    .Where(sourceId => !sourceId.Contains("live-betting")).ToList();

                await UpdateScrapeStatus(10, "Scraping metric data");

                var rangeProgress = sourceIds.Count != 0 ? 90 / sourceIds.Count : 0;
                var currentRange = 10;
                foreach (var matchUrl in sourceIds.Select(sourceId => $"https://nj.betamerica.com{sourceId}"))
                {
                    chromeDriver.Navigate().GoToUrl(matchUrl);
                    await Task.Delay(7000);
                    doc.LoadHtml(chromeDriver.PageSource);

                    currentRange = Math.Min(currentRange + rangeProgress, 90);

                    var rawMetrics = doc.DocumentNode.SelectNodes("//html/body/div[@class='content-main']/div[contains(@class, 'content-main-inner')]/div[@id='pagesWrapper']/div[@id='panel-center-inner']/section[@id='pre-live-betting']/div/div[@class='event-view-views-switcher']/div/ul");
                    foreach (var rawMetric in rawMetrics)
                    {
                        var liTags = rawMetric.SelectNodes("li");
                        foreach (var liTag in liTags)
                        {
                            var spanItem = liTag.SelectSingleNode("h4/span[2]/span");
                            var spanText = spanItem.InnerText;

                            var scoreType =
                                spanText.Contains("Total Points") ? ScoreType.Point :
                                spanText.Contains("Total Rebounds") ? ScoreType.Rebound :
                                spanText.Contains("Total Assists") ? ScoreType.Assist :
                                spanText.Contains("Pts, Rebs, Asts") ? ScoreType.PointReboundAssist :
                                string.Empty;

                            if (string.IsNullOrEmpty(scoreType)) continue;

                            var playerName = ScrapeHelper.RegexMappingExpression(spanText, @"(.*) (Total|\(Pts,)");
                            var match = ScrapeHelper.FindMatchByPlayerName(playerName, TodayMatches);
                            if (match == null)
                            {
                                continue;
                            }

                            var player = ScrapeHelper.FindPlayerInMatch(playerName, match);

                            if (player == null)
                            {
                                Logger.Warning($"Cannot find any player {playerName} in match {match.Id}");
                                continue;
                            }

                            var overUnderFirstData = liTag.SelectSingleNode("div/div/div[2]/button");
                            var overUnderSecondData = liTag.SelectSingleNode("div/div/div[1]/button");

                            double? over = 0, overLine = 0, under = 0, underLine = 0;

                            var overUnderFirstLine = overUnderFirstData.SelectSingleNode("span[1]/span[1]").InnerText;
                            if (overUnderFirstLine.Contains("Over"))
                            {
                                overLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(overUnderFirstLine, @"Over (\d*.\d*) .*"));
                                var overPriceData = overUnderFirstData.SelectSingleNode("span[2]/span").InnerText;
                                var priceNumber = ScrapeHelper.ConvertMetric(overPriceData);
                                over = priceNumber != null
                                    ? priceNumber > 0 ? Math.Round((double) (priceNumber / 100 + 1), 2) :
                                    Math.Round((double) (100 / (priceNumber * -1) + 1), 2)
                                    : (double?) null;
                            }
                            else
                            {

                                underLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(overUnderFirstLine, @"Under (\d*.\d*) .*"));
                                var underPriceData = overUnderFirstData.SelectSingleNode("span[2]/span").InnerText;
                                var priceNumber = ScrapeHelper.ConvertMetric(underPriceData);
                                under = priceNumber != null
                                    ? priceNumber > 0 ? Math.Round((double)(priceNumber / 100 + 1), 2) :
                                    Math.Round((double)(100 / (priceNumber * -1) + 1), 2)
                                    : (double?)null;
                            }

                            var overUnderSecondLine = overUnderSecondData.SelectSingleNode("span[1]/span[1]").InnerText;

                            if (overUnderSecondLine.Contains("Over"))
                            {
                                overLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(overUnderSecondLine, @"Over (\d*.\d*) .*"));
                                var overPriceData = overUnderSecondData.SelectSingleNode("span[2]/span").InnerText;
                                var priceNumber = ScrapeHelper.ConvertMetric(overPriceData);
                                over = priceNumber != null
                                    ? priceNumber > 0 ? Math.Round((double)(priceNumber / 100 + 1), 2) :
                                    Math.Round((double)(100 / (priceNumber * -1) + 1), 2)
                                    : (double?)null;
                            }
                            else
                            {
                                underLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(overUnderSecondLine, @"Under (\d*.\d*) .*"));
                                var underPriceData = overUnderSecondData.SelectSingleNode("span[2]/span").InnerText;
                                var priceNumber = ScrapeHelper.ConvertMetric(underPriceData);
                                under = priceNumber != null
                                    ? priceNumber > 0 ? Math.Round((double)(priceNumber / 100 + 1), 2) :
                                    Math.Round((double)(100 / (priceNumber * -1) + 1), 2)
                                    : (double?)null;
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
                        }

                        var newProgress = GetScrapingInformation().Progress;
                        newProgress = Math.Min(newProgress + currentRange / rawMetrics.Count, currentRange);
                        await UpdateScrapeStatus(newProgress, null);
                    }
                    await UpdateScrapeStatus(currentRange, null);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                chromeDriver.Quit();
                throw;
            }
            chromeDriver.Quit();
        }
    }
}
