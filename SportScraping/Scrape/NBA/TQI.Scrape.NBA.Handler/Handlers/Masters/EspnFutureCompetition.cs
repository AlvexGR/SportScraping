﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Serilog;
using TQI.Infrastructure.Entity.Models;
using TQI.Infrastructure.Scrape.Handler;
using TQI.Infrastructure.Utility;

namespace TQI.Scrape.NBA.Handler.Handlers.Masters
{
    public class EspnFutureCompetition : ScrapeHandler
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public EspnFutureCompetition(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
            : base(logger, webPortalHelper, scrapeHelper)
        {
        }

        public EspnFutureCompetition()
        {
            FromDate = DateTime.Now;
            ToDate = DateTime.Now.AddDays(5);
        }

        protected override async Task ScrapeData()
        {
            // Fetch all teams from web portal
            var teams = await WebPortalHelper.GetTeams(Helper.GetSportCode());
            if (teams == null || teams.Count == 0)
            {
                throw new ArgumentNullException(nameof(teams), "Teams from web portal is null");
            }
            FromDate = Helper.ToMinTime(FromDate);
            ToDate = Helper.ToMaxTime(ToDate);
            var totalDays = Convert.ToInt32((ToDate - FromDate).TotalDays);

            for (var date = FromDate.Date; date <= ToDate.Date; date = date.AddDays(1))
            {
                Logger.Information($"Scrape matches: {date.ToShortDateString()}");
                await UpdateScrapeStatus(null, $"Scrape matches: {date.ToShortDateString()}");
                var url = $"http://site.api.espn.com/apis/site/v2/sports/basketball/nba/scoreboard?lang=en&region=au&calendartype=blacklist&limit=100&dates={Helper.GetDate(date)}&tz=Australia%2FMelbourne";
                var rawResult = await ScrapeHelper.GetDocument(url);
                var jObject = JObject.Parse(rawResult);
                if (jObject == null)
                {
                    throw new ArgumentNullException(nameof(jObject));
                }

                var sportCode = Helper.GetSportCode();
                var teamCodes = new List<string>();
                var competitions = jObject.SelectTokens("$.events[*].competitions[*]");
                foreach (var competition in competitions)
                {
                    var homeAbbr = competition.SelectToken("$.competitors[0].team.abbreviation").ToString();
                    var awayAbbr = competition.SelectToken("$.competitors[1].team.abbreviation").ToString();

                    if (teams.All(x => x.ShortName != homeAbbr))
                    {
                        throw new Exception($"Cannot find any home team named '{homeAbbr}' in teams");
                    }
                    if (teams.All(x => x.ShortName != awayAbbr))
                    {
                        throw new Exception($"Cannot find any away team named '{awayAbbr}' in teams");
                    }

                    teamCodes.Add(homeAbbr);
                    teamCodes.Add(awayAbbr);

                    var homeTeamId = teams.First(x => x.ShortName.Equals(homeAbbr)).Id;
                    var awayTeamId = teams.First(x => x.ShortName.Equals(awayAbbr)).Id;

                    var homeTeamName = competition.SelectToken("$.competitors[0].team.name").ToString();
                    var awayTeamName = competition.SelectToken("$.competitors[1].team.name").ToString();
                    homeTeamName = homeTeamName.Substring(0, Math.Min(homeTeamName.Length, 3)).ToUpper();
                    awayTeamName = awayTeamName.Substring(0, Math.Min(awayTeamName.Length, 3)).ToUpper();

                    var gameCode = $"{Helper.GetDate(date, "MMddyyyy")}{homeTeamName}{awayTeamName}";
                    var gameDate = competition.SelectToken("$.date").ToString();
                    Logger.Information($"Match: {gameCode}, {homeTeamName} vs {awayTeamName}, {gameDate}");
                    Matches.Add(new Match
                    {
                        StartTime = DateTime.Parse(gameDate),
                        HomeTeamId = homeTeamId,
                        AwayTeamId = awayTeamId,
                        GameCode = gameCode,
                        SportCode = sportCode
                    });
                }
                Logger.Information("Scraped matches complete");

                Logger.Information("Scrape players from teams");
                const string baseTeamsUrl = "https://www.espn.com/nba/team/stats/_/name";
                const string xPathToPlayers = "/html/body/div[1]/div/div/div/div/div[5]/div[2]/div[5]/div[1]/div/section/div/section[1]/div[2]/table/tbody/tr[*]/td/span/a";
                var playerTasks = new List<Task<HtmlNodeCollection>>();
                foreach (var teamCode in teamCodes)
                {
                    url = $"{baseTeamsUrl}/{teamCode}";
                    playerTasks.Add(ScrapeHelper.GetInnerHtml(url, xPathToPlayers));
                }

                var nodes = await Task.WhenAll(playerTasks);

                for (var i = 0; i < teamCodes.Count; i++)
                {
                    var teamId = teams.First(x => x.ShortName == teamCodes[i]).Id;
                    Logger.Information($"Scrape player from: {baseTeamsUrl}/{teamCodes[i]}");
                    ExtractPlayers(nodes[i], teamId);
                }

                Logger.Information("Scrape players complete");
                var newProgress = GetScrapingInformation().Progress;
                newProgress = Math.Min(newProgress + 90 / totalDays, 90);
                await UpdateScrapeStatus(newProgress, $"Scrape matches: {date.ToShortDateString()} complete");
            }
        }

        private void ExtractPlayers(HtmlNodeCollection playerElements, int teamId)
        {
            for (var playerIdx = 0; playerIdx < playerElements.Count; playerIdx++)
            {
                var item = playerElements[playerIdx];
                const int sourceIdInHref = 7;
                var sourceId = item.Attributes["href"].Value.Split('/')[sourceIdInHref];
                var playerName = item.InnerText;
                playerName = ScrapeHelper.FormatPlayerName(playerName);
                Logger.Information($"SourceId {sourceId}, PlayerName {playerName}, TeamId {teamId}");
                if (string.IsNullOrEmpty(sourceId) || string.IsNullOrEmpty(playerName))
                {
                    Logger.Warning($"SourceId {sourceId} or PlayerName {playerName} " +
                                   $"is null or empty at index {playerIdx}");
                    continue;
                }

                Players.Add(new Player
                {
                    SourceId = sourceId,
                    Name = playerName,
                    TeamId = teamId
                });
            }
        }
    }
}
