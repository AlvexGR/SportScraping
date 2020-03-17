using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using Serilog;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Entity.Models.Metrics;
using TQI.Infrastructure.Scrape.Handler;
using TQI.Infrastructure.Utility;

namespace TQI.Scrape.NBA.Handler.Handlers.Metrics.PlayerOverUnders
{
    public class Bet365PlayerOverUnder : ScrapeHandler
    {
        public Bet365PlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public Bet365PlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            var chromeDriver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            try
            {
                var scoreTypes = new[]
                {
                    (ScoreType.Point, "Player Points"),
                    (ScoreType.Assist, "Player Assists"),
                    (ScoreType.Rebound, "Player Rebounds"),
                    (ScoreType.PointAssist, "Player Points and Assists"),
                    (ScoreType.PointRebound, "Player Points and Rebounds"),
                    (ScoreType.ReboundAssist, "Player Assists and Rebounds"),
                    (ScoreType.PointReboundAssist, "Player Points, Assists and Rebounds")
                };

                await UpdateScrapeStatus(null, "Scraping metric data");

                foreach (var (item1, item2) in scoreTypes)
                {
                    await ProcessMetric(chromeDriver, item1, item2);
                    var newProgress = GetScrapingInformation().Progress;
                    newProgress = Math.Min(newProgress + 90 / scoreTypes.Length, 90);
                    await UpdateScrapeStatus(newProgress, null);
                }

                await UpdateScrapeStatus(null, "Scrape metric data complete");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                chromeDriver.Quit();
                throw;
            }
            chromeDriver.Quit();
        }

        private async Task ProcessMetric(RemoteWebDriver chromeDriver, string scoreType, string actualScoreType)
        {
            const string url = "https://www.bet365.com.au/";
            chromeDriver.Navigate().GoToUrl(url);
            await Task.Delay(10000);

            var element =
                chromeDriver.FindElementByXPath(
                    "//div[@class='wn-Classification ' and contains(text(), 'Basketball')]");
            element.Click();
            await Task.Delay(5000);

            var pointElement =
                chromeDriver.FindElementByXPath(
                    $"//span[@class='sm-CouponLink_Title ' and contains(text(), '{actualScoreType}')]");
            pointElement.Click();
            await Task.Delay(5000);

            Logger.Information($"Scraping {scoreType}");

            var doc = new HtmlDocument();
            doc.LoadHtml(chromeDriver.PageSource);

            var mainPage = doc.DocumentNode.SelectSingleNode("//html/body/div[1]/div[1]/div[@class='wc-PageView ']/div[@class='wc-PageView_Main ']/div/div[@class='wcl-CommonElementStyle_PrematchCenter ']/div[@class='cm-CouponModule ']");
            var playerElements = mainPage.SelectNodes("//div[contains(@class,'cm-MarketCouponValuesExplicit21')]/div[@class='gll-Participant_General sl-CouponParticipantPlayerTeam ']");
            var overElements = mainPage.SelectNodes("//div[contains(@class,'cm-MarketCouponValuesExplicit22') and not(contains(@class, 'gll-Market_LastInRow'))]/div[contains(@class,'gll-ParticipantCentered')]");
            var underElements = mainPage.SelectNodes("//div[contains(@class,'cm-MarketCouponValuesExplicit22') and contains(@class, 'gll-Market_LastInRow')]/div[contains(@class,'gll-ParticipantCentered')]");

            var index = 0;
            foreach (var playerElement in playerElements)
            {
                var playerName = playerElement.SelectSingleNode("span['sl-CouponParticipantPlayerTeam_Name']").InnerText;
                var match = ScrapeHelper.FindMatchByPlayerName(playerName, TodayMatches);
                if (match == null)
                {
                    index++;
                    continue;
                }

                var player = ScrapeHelper.FindPlayerInMatch(playerName, match);

                var overItem = overElements[index];
                var overLine = ScrapeHelper.ConvertMetric(overItem.SelectSingleNode("span[contains(@class, 'gll-ParticipantCentered_Name')]").InnerText);
                var over = ScrapeHelper.ConvertMetric(overItem.SelectSingleNode("span[contains(@class, 'gll-ParticipantCentered_Odds')]").InnerText);
                var underItem = underElements[index];
                var underLine = ScrapeHelper.ConvertMetric(underItem.SelectSingleNode("span[contains(@class, 'gll-ParticipantCentered_Name')]").InnerText);
                var under = ScrapeHelper.ConvertMetric(underItem.SelectSingleNode("span[contains(@class, 'gll-ParticipantCentered_Odds')]").InnerText);

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
                index++;
            }
        }
    }
}
