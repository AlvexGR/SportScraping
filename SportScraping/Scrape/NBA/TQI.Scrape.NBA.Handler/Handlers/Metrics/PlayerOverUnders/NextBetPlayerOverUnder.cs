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
    public class NextBetPlayerOverUnder : ScrapeHandler
    {
        public NextBetPlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public NextBetPlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            var chromeDriver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            try
            {
                const string url = "https://www.nextbet.com/en/sports/227-basketball/22892-usa/44178-nba";
                chromeDriver.Navigate().GoToUrl(url);
                await Task.Delay(10000);

                var doc = new HtmlDocument();
                doc.LoadHtml(chromeDriver.PageSource);
                var matchNodes = doc.DocumentNode.SelectNodes("//span[@class='period-description']");
                var rawMatches = matchNodes.Where(x => !x.InnerText.Contains("Tomorrow")).ToList();

                Logger.Information("Scrape metric data");
                await UpdateScrapeStatus(10, "Scraping metric data");

                var rangeProgress = rawMatches.Count != 0 ? 90 / rawMatches.Count : 0;
                var currentRange = 10;
                for (var i = 0; i < rawMatches.Count; i++)
                {
                    chromeDriver.Navigate().GoToUrl(url);
                    await Task.Delay(10000);
                    var markets = chromeDriver.FindElementsByClassName("more_markets");
                    markets[i].Click();
                    await Task.Delay(10000);
                    doc.LoadHtml(chromeDriver.PageSource);

                    var marketContainer = doc.DocumentNode.SelectSingleNode("//div[@id='content']");
                    var marketGroups = marketContainer.SelectNodes("//div[@class='markets-group-component']");
                    currentRange = Math.Min(currentRange + rangeProgress, 90);

                    foreach (var marketItem in marketGroups)
                    {
                        string scoreTypeItem;
                        try
                        {
                            scoreTypeItem = marketItem.SelectSingleNode("div/div/h2").InnerText;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        var scoreType =
                            scoreTypeItem.Contains("Points + Assists") ? ScoreType.PointAssist :
                            scoreTypeItem.Contains("Points + Rebounds") ? ScoreType.PointRebound :
                            scoreTypeItem.Contains("Assists + Rebounds") ? ScoreType.ReboundAssist :
                            scoreTypeItem.Contains("Points + Assists + Rebounds") ? ScoreType.PointReboundAssist :
                            scoreTypeItem.Contains("Points") ? ScoreType.Point :
                            scoreTypeItem.Contains("Rebounds") ? ScoreType.Rebound :
                            scoreTypeItem.Contains("Assists") ? ScoreType.Assist :
                            scoreTypeItem.Contains("Three Made") ? ScoreType.ThreePoint :
                            string.Empty;

                        if (string.IsNullOrEmpty(scoreType) || scoreTypeItem.Contains("Odd/Even") ||
                            !scoreTypeItem.Contains("Match")) continue;

                        doc.LoadHtml(marketItem.InnerHtml);
                        var playerMarkets = doc.DocumentNode.SelectNodes("//div[@class='market-component']");

                        foreach (var playerMarket in playerMarkets)
                        {
                            var playerName = playerMarket.SelectSingleNode("div[@class='player']").InnerText
                                .Replace("\n", string.Empty)
                                .Replace("\r", string.Empty)
                                .Replace("\t", string.Empty)
                                .Trim();

                            var match = ScrapeHelper.FindMatchByPlayerName(playerName, TodayMatches);
                            if (match == null)
                            {
                                continue;
                            }

                            var player = ScrapeHelper.FindPlayerInMatch(playerName, match);

                            var overLineItem = playerMarket.SelectSingleNode("div[@class='swish-markets-wrapper']/div/table/tbody/tr/td/span");
                            var overLineData = overLineItem.InnerText.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty).Trim();
                            var overLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(overLineData, "Over (.*)"));

                            var priceOverData = playerMarket.SelectSingleNode("div[@class='swish-markets-wrapper']/div/table/tbody/tr/td/span[2]/span/span");
                            var over = ScrapeHelper.ConvertMetric(priceOverData.InnerText.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty).Trim());

                            var underLineItem = playerMarket.SelectSingleNode("div[@class='swish-markets-wrapper']/div/table/tbody/tr/td[2]/span");
                            var underLineData = underLineItem.InnerText.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty).Trim();
                            var underLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(underLineData, "Under (.*)"));

                            var priceUnderData = playerMarket.SelectSingleNode("div[@class='swish-markets-wrapper']/div/table/tbody/tr/td[2]/span[2]/span/span");
                            var under = ScrapeHelper.ConvertMetric(priceUnderData.InnerText.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty).Trim());

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
                            newProgress = Math.Min(newProgress + currentRange / playerMarkets.Count, currentRange);
                            await UpdateScrapeStatus(newProgress, null);
                        }
                    }

                    await UpdateScrapeStatus(currentRange, null);
                }
                Logger.Information("Scrape metric data complete");
                await UpdateScrapeStatus(90, "Scrape metric data complete");
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
