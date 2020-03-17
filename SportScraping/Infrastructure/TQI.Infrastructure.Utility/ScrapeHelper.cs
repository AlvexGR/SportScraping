using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Serilog;
using TQI.Infrastructure.Entity.Models;
using Match = TQI.Infrastructure.Entity.Models.Match;

namespace TQI.Infrastructure.Utility
{
    public class ScrapeHelper
    {
        private readonly HttpClient _client;
        private readonly WebClient _webClient;

        public ILogger Logger { get; set; }

        public ScrapeHelper(HttpClient client, WebClient webClient)
        {
            _client = client;
            _webClient = webClient;
        }

        /// <summary>
        /// Get string document from url
        /// </summary>
        /// <param name="url">Document url</param>
        /// <param name="timeout">Delay time in milliseconds</param>
        /// <returns></returns>
        public async Task<string> GetDocument(string url, int timeout = 0)
        {
            if (string.IsNullOrEmpty(url))
            {
                Logger.Warning("Input url is null or empty");
                return null;
            }

            Logger.Information($"Get document from {url}");

            if (timeout > 0)
            {
                Logger.Information($"Delay getting document for {timeout}ms");
                await Task.Delay(timeout);
            }

            var result = await _client.GetStringAsync(url);
            return result;
        }

        /// <summary>
        /// Get string document from uri
        /// </summary>
        /// <param name="uri">Document uri</param>
        /// <param name="timeout">Delay time in milliseconds</param>
        /// <returns></returns>
        public async Task<string> GetDocument(Uri uri, int timeout = 0)
        {
            if (uri == null)
            {
                Logger.Warning("Input url is null or empty");
                return null;
            }

            Logger.Information($"Get document from {uri.AbsoluteUri}");

            if (timeout > 0)
            {
                Logger.Information($"Delay getting document for {timeout}ms");
                await Task.Delay(timeout);
            }
            _webClient.Headers.Clear();
            _webClient.Headers.Add("cache-control", "no-cache");
            _webClient.Headers.Add("content-type", "application/json");
            var result = await _webClient.DownloadStringTaskAsync(uri);
            return result;
        }

        /// <summary>
        /// Get IWebElements from url with Xpath by Chrome Driver
        /// </summary>
        /// <param name="url">Url to get</param>
        /// <param name="xPath">xPath to query</param>
        /// <param name="timeout">Delay time in milliseconds</param>
        /// <returns>ReadOnlyCollection of IWebElement</returns>
        public async Task<ReadOnlyCollection<IWebElement>> GetElementsByXPath(string url, string xPath, int timeout = 0)
        {
            if (string.IsNullOrEmpty(url))
            {
                Logger.Warning("Input url is null or empty");
                return null;
            }

            if (string.IsNullOrEmpty(xPath))
            {
                Logger.Warning("Input xPath is null or empty");
                return null;
            }
            if (timeout > 0)
            {
                Logger.Information($"Delay getting document for {timeout}ms");
                await Task.Delay(timeout);
            }
            var chromeDriver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            chromeDriver.Navigate().GoToUrl(url);

            var result = chromeDriver.FindElementsByXPath(xPath);

            chromeDriver.Quit();

            return result;
        }

        /// <summary>
        /// Get inner html with xpath
        /// </summary>
        /// <param name="url">Url to get</param>
        /// <param name="xPath">xPath to query</param>
        /// <param name="timeout">Delay time in milliseconds</param>
        /// <returns>HtmlNodeCollection from url</returns>
        public async Task<HtmlNodeCollection> GetInnerHtml(string url, string xPath, int timeout = 0)
        {
            var document = await GetDocument(url, timeout);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(document);
            if (string.IsNullOrEmpty(xPath))
            {
                Logger.Warning("Input xPath is null or empty");
                return null;
            }

            var node = htmlDoc.DocumentNode.SelectNodes(xPath);
            return node;
        }

        /// <summary>
        /// Get by page source with xPath
        /// </summary>
        /// <param name="pageSource">Page source</param>
        /// <param name="xPath">xPath to query</param>
        /// <returns>HtmlNodeCollection from pageSource</returns>
        public HtmlNodeCollection GetInnerHtmlByPageSource(string pageSource, string xPath)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageSource);
            if (string.IsNullOrEmpty(xPath))
            {
                Logger.Warning("Input xPath is null or empty");
                return null;
            }

            var node = htmlDoc.DocumentNode.SelectNodes(xPath);
            return node;
        }

        public string RegexMappingExpression(string name, string regexExpression)
        {
            if (string.IsNullOrEmpty(name))
            {
                Logger.Warning("Input name is null or empty");
                return null;
            }
            if (string.IsNullOrEmpty(regexExpression))
            {
                Logger.Warning("Input regexExpression is null or empty");
                return null;
            }
            var regex = new Regex(regexExpression);
            var regexMatch = regex.Match(name);
            return regexMatch.Success ? regexMatch.Groups[1].Value : string.Empty;
        }

        public Match FindMatchByHomeAndAwayTeam(List<Match> matches, string homeTeam, string awayTeam)
        {
            return matches.FirstOrDefault(x =>
                x.HomeTeam.LongName.CompareName(homeTeam) && x.AwayTeam.LongName.CompareName(awayTeam)
                || x.HomeTeam.ShortName.CompareName(homeTeam) && x.AwayTeam.ShortName.CompareName(awayTeam)
                || x.HomeTeam.LongName.Contains(homeTeam) && x.AwayTeam.LongName.Contains(awayTeam)
                || homeTeam.Contains(x.HomeTeam.ShortName) && awayTeam.Contains(x.AwayTeam.ShortName));
        }

        public Player FindPlayerInMatch(string playerName, Match match, bool applyLcs = true)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                Logger.Warning("playerName is null or empty");
                return null;
            }

            var player = match.HomeTeam.Players.FirstOrDefault(x =>
                             x.Name.CompareName(playerName) || playerName.CompareName(x.Name)) ??
                         match.AwayTeam.Players.FirstOrDefault(x =>
                             x.Name.CompareName(playerName) || playerName.CompareName(x.Name));

            if (player != null || !applyLcs) return player;

            // Find with LCS algorithm
            var preprocessLcs = match.HomeTeam.Players.Select(x => playerName.LongestCommonSubsequence(x.Name)).ToList();
            preprocessLcs.AddRange(match.AwayTeam.Players.Select(x => playerName.LongestCommonSubsequence(x.Name)));

            var maxLength = preprocessLcs.Max(y => y.Length);
            var playersByLcs = match.HomeTeam.Players
                .Where(x => playerName.LongestCommonSubsequence(x.Name).Length == maxLength).ToList();
            playersByLcs.AddRange(match.AwayTeam.Players.Where(x =>
                playerName.LongestCommonSubsequence(x.Name).Length == maxLength));

            return playersByLcs.FirstOrDefault();
        }

        public Match FindMatchByPlayerName(string playerName, List<Match> matches)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                Logger.Warning("playerName is null or empty");
                return null;
            }
            var foundMatches = matches.Where(x => FindPlayerInMatch(playerName, x, false) != null).ToList();
            if (foundMatches.Count == 1)
            {
                return foundMatches.FirstOrDefault();
            }

            // Find with LCS algorithm
            var preprocessLcs = new List<string>();
            foreach (var match in matches)
            {
                preprocessLcs.AddRange(match.HomeTeam.Players.Select(x => playerName.LongestCommonSubsequence(x.Name)));
                preprocessLcs.AddRange(match.AwayTeam.Players.Select(x => playerName.LongestCommonSubsequence(x.Name)));
            }
            var maxLength = preprocessLcs.Max(y => y.Length);
            var matchesByLcs = matches
                .Where(match =>
                    match.HomeTeam.Players.Any(x => playerName.LongestCommonSubsequence(x.Name).Length == maxLength)
                    || match.AwayTeam.Players.Any(x => playerName.LongestCommonSubsequence(x.Name).Length == maxLength))
                .ToList();

            if (matchesByLcs.Count == 1) return matchesByLcs.FirstOrDefault();

            Logger.Warning($"Total matches contain {playerName} is {matchesByLcs.Count}");
            return null;
        }

        public double? ConvertMetric(string toConvert)
        {
            try
            {
                return Convert.ToDouble(toConvert);
            }
            catch
            {
                Logger.Warning($"Cannot convert {toConvert}");
                return null;
            }
        }

        public string FormatPlayerName(string playerName)
        {
            if (string.IsNullOrEmpty(playerName)) return playerName;
            var formattedPlayerName = playerName.Replace("&#x27;", "'");
            return formattedPlayerName;
        }
    }
}
