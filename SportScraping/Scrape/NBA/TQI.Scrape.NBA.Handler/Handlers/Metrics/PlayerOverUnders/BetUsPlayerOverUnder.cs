using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Serilog;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Entity.Models.Metrics;
using TQI.Infrastructure.Scrape.Handler;
using TQI.Infrastructure.Utility;

namespace TQI.Scrape.NBA.Handler.Handlers.Metrics.PlayerOverUnders
{
    public class BetUsPlayerOverUnder : ScrapeHandler
    {
        public BetUsPlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public BetUsPlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            var chromeDriver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            try
            {
                const string url = "https://www.betus.com.pa/sportsbook/nba-basketball-lines.aspx";
                chromeDriver.Navigate().GoToUrl(url);
                await Task.Delay(5000);
                var matchCount = chromeDriver.FindElementsByClassName("props").Count;
                await UpdateScrapeStatus(10, "Scraping metric data");

                var rangeProgress = matchCount != 0 ? 90 / matchCount : 0;
                var currentRange = 10;
                for (var i = 0; i < matchCount; i++)
                {
                    chromeDriver.Navigate().GoToUrl(url);
                    await Task.Delay(3000);
                    var props = chromeDriver.FindElementsByClassName("props");
                    props[i].Click();
                    await Task.Delay(3000);

                    var select = new SelectElement(chromeDriver.FindElementByClassName("odds-display"));
                    select.SelectByText("Decimal");
                    await Task.Delay(2000);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(chromeDriver.PageSource);

                    currentRange = Math.Min(currentRange + rangeProgress, 90);

                    var matchNode = doc.DocumentNode.SelectSingleNode("//html/body/form/div[@class='sportsbook-visitor']/div[@class='sportsbook-master']/div[@class='rounded-block-white']/span[2]/div[@class='col']/div[@id='game-lines']/div/div[@class='game-block']/div[@class='normal']/div/div[@class='future-lines inline-prop']");
                    var rawMetrics = matchNode.SelectNodes("table");

                    foreach (var rawMetric in rawMetrics)
                    {
                        var playerNode = rawMetric.SelectSingleNode("tbody/tr[1]/th[2]").InnerText;
                        var scoreType =
                            playerNode.Contains("Total Points+Rebounds+Assists") ? ScoreType.PointReboundAssist :
                            playerNode.Contains("Total Points+Rebounds") ? ScoreType.PointRebound :
                            playerNode.Contains("Total Points+Assists") ? ScoreType.PointAssist :
                            playerNode.Contains("Total Rebounds+Assists") ? ScoreType.ReboundAssist :
                            playerNode.Contains("Total Points") ? ScoreType.Point :
                            playerNode.Contains("Total Rebounds") ? ScoreType.Rebound :
                            playerNode.Contains("Total Assists") ? ScoreType.Assist :
                            playerNode.Contains("Total Made 3") ? ScoreType.ThreePoint :
                            string.Empty;

                        if (string.IsNullOrEmpty(scoreType)) continue;

                        playerNode = playerNode
                            .Replace("\n", string.Empty)
                            .Replace("\r", string.Empty)
                            .Replace("\t", string.Empty)
                            .Trim();
                        var playerName = ScrapeHelper.RegexMappingExpression(playerNode, @"(.*) \(");
                        var match = ScrapeHelper.FindMatchByPlayerName(playerName, TodayMatches);
                        if (match == null)
                        {
                            continue;
                        }

                        var player = ScrapeHelper.FindPlayerInMatch(playerName, match);

                        var overNode = rawMetric.SelectSingleNode("tbody/tr[2]/td[2]").InnerText;
                        overNode = overNode
                            .Replace("\n", string.Empty)
                            .Replace("\r", string.Empty)
                            .Replace("\t", string.Empty)
                            .Replace("&nbsp;", " ")
                            .Trim();
                        var overLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(overNode, @"Over.(\d*.\d*)"));

                        var overPriceNode = rawMetric.SelectSingleNode("tbody/tr[2]/td[3]/a").InnerText;
                        var over = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(overPriceNode, @"(\d*.\d*).*"));

                        var underNode = rawMetric.SelectSingleNode("tbody/tr[3]/td[2]").InnerText;
                        underNode = underNode
                            .Replace("\n", string.Empty)
                            .Replace("\r", string.Empty)
                            .Replace("&nbsp;", " ")
                            .Replace("\t", string.Empty)
                            .Trim();
                        var underLine = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(underNode, @"Under.(\d*.\d*)"));
                        var underPriceNode = rawMetric.SelectSingleNode("tbody/tr[3]/td[3]/a").InnerText;
                        var under = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(underPriceNode, @"(\d*.\d*).*"));

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
