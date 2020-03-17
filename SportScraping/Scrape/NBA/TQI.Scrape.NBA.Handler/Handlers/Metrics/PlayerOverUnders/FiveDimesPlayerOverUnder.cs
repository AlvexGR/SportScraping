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
    public class FiveDimesPlayerOverUnder : ScrapeHandler
    {
        public FiveDimesPlayerOverUnder(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public FiveDimesPlayerOverUnder()
        {
        }

        protected override async Task ScrapeData()
        {
            var chromeDriver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            try
            {
                const string url = "https://www.5dimes.eu/";
                chromeDriver.Navigate().GoToUrl(url);
                await Task.Delay(2000);
                var userElement = chromeDriver.FindElementById("customerID");
                userElement.SendKeys("5D2543319");
                var passwordElement = chromeDriver.FindElementByName("password");
                passwordElement.SendKeys("qwerty123!");
                var loginButton = chromeDriver.FindElementById("submit1");
                loginButton.Click();
                await Task.Delay(2000);
                var nbaCheckbox = chromeDriver.FindElementByName("Basketball_NBA Props");
                nbaCheckbox.Click();

                var nbaSubmitElement = chromeDriver.FindElementById("btnContinue");
                nbaSubmitElement.Click();

                var doc = new HtmlDocument();
                doc.LoadHtml(chromeDriver.PageSource);

                var rawMatches = doc.DocumentNode.SelectNodes("//html/body/form/div[@id='PageElt']/div/div[@id='Middle']/div[@id='contentRight']/div[@id='ManagedContentWrapper']/div[@id='MainContent']/div[@id='mainContentContainer']/div[@id='readingPaneSplitPane']/div[@id='readingPaneContainer']/div[@id='readingPaneContentContainer']/div[@id='contentContainer']/div[@class='linesContainer']/table/tbody/tr");
                for (var i = 0; i < rawMatches.Count; i++)
                {
                    var tdScoreName = rawMatches[i].SelectSingleNode("td[3]");
                    if (tdScoreName == null) continue;
                    var scoreText = tdScoreName.InnerText;
                    var scoreType =
                        scoreText.Contains("points") && !scoreText.Contains("first") && !scoreText.Contains("total") && !scoreText.Contains("wins") ? ScoreType.Point :
                        scoreText.Contains("assists") ? ScoreType.Assist :
                        scoreText.Contains("rebounds") ? ScoreType.Rebound :
                        scoreText.Contains("3-Pt") ? ScoreType.ThreePoint :
                        scoreText.Contains("pts+reb+ast") ? ScoreType.PointReboundAssist :
                        scoreText.Contains("pts+reb") ? ScoreType.PointRebound :
                        scoreText.Contains("pts+ast") ? ScoreType.PointAssist :
                        scoreText.Contains("reb+ast") ? ScoreType.ReboundAssist
                        : string.Empty;
                    if (string.IsNullOrEmpty(scoreType)) continue;

                    var scoreSplits = scoreText.Split();
                    var playerName = scoreSplits[1].Replace(".", " ");

                    var match = ScrapeHelper.FindMatchByPlayerName(playerName, TodayMatches);
                    if (match == null)
                    {
                        continue;
                    }
                    var player = ScrapeHelper.FindPlayerInMatch(playerName, match);

                    var overText = rawMatches[i].SelectSingleNode("td[6]").InnerText;
                    var overLine = ScrapeHelper.ConvertMetric($"{ScrapeHelper.RegexMappingExpression(overText, "o(.*)½")}.05");
                    var over = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(overText, @"½ [\-\+](.*)").Trim());

                    var underText = rawMatches[i + 1].SelectSingleNode("td[6]").InnerText;
                    var underLine = ScrapeHelper.ConvertMetric($"{ScrapeHelper.RegexMappingExpression(underText, "u(.*)½")}.05");
                    var under = ScrapeHelper.ConvertMetric(ScrapeHelper.RegexMappingExpression(underText, @"½ [\-\+](.*)").Trim());

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
