using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Entity.Models;
using TQI.Infrastructure.Entity.Models.Metrics;

namespace TQI.Infrastructure.Utility
{
    /// <summary>
    /// Handle web portal apis
    /// </summary>
    public class WebPortalHelper
    {
        private readonly HttpClient _client;
        
        public ILogger Logger { get; set; }

        private List<Match> _todayMatches;

        private List<Provider> _providers;

        private const string WebPortalDateTimeFormat = "yyyy-MM-ddTHH:mm:ss";

        public WebPortalHelper(HttpClient client)
        {
            _client = client;

            // Avoid trust relationship for the SSL/TLS secure channel issue
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        /// <summary>
        /// Base get method for web portal
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="url">Web portal api route</param>
        /// <returns>Object T</returns>
        private async Task<T> GetAsync<T>(string url) where T : class
        {
            var response = await _client.GetAsync(url);
            T result;
            if (response.IsSuccessStatusCode)
            {
                var apiResult = JsonConvert.DeserializeObject<ApiResult<T>>
                    (await response.Content.ReadAsStringAsync());
                if (!apiResult.Succeed)
                {
                    throw new Exception($"Web portal responds: {apiResult.Error}");
                }
                result = apiResult.Result;
            }
            else
            {
                throw new Exception($"Web portal responds with code {response.StatusCode}: " +
                                    $"{JsonConvert.SerializeObject(response)}");
            }

            return result;
        }

        /// <summary>
        /// Base post method for web portal
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="url">Web portal api route</param>
        /// <param name="rawData">Data needed</param>
        /// <returns>Object T</returns>
        private async Task<T> PostAsync<T>(string url, object rawData)
        {
            var jData = string.Empty;
            if (rawData != null)
            {
                jData = JsonConvert.SerializeObject(rawData);
            }
            var content = new StringContent(jData, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);
            T result;
            if (response.IsSuccessStatusCode)
            {
                var apiResult = JsonConvert.DeserializeObject<ApiResult<T>>
                    (await response.Content.ReadAsStringAsync());
                if (!apiResult.Succeed)
                {
                    throw new Exception($"Web portal responds: {apiResult.Error}");
                }
                result = apiResult.Result;
            }
            else
            {
                throw new Exception($"Web portal responds with code {response.StatusCode}: " +
                                    $"{JsonConvert.SerializeObject(response)}");
            }

            return result;
        }

        /// <summary>
        /// Get providers by sport code
        /// </summary>
        /// <returns></returns>
        public async Task<List<Provider>> GetProviders()
        {
            var url = $"{Constants.WebPortalEndpoint}/Sport/Provider/{Helper.GetSportCode()}";
            Logger.Information($"Get providers data from {url}");
            return await GetAsync<List<Provider>>(url);
        }

        /// <summary>
        /// Get matches by date range and sport code
        /// </summary>
        /// <param name="from">From date</param>
        /// <param name="to">To date</param>
        /// <param name="sportCode">Sport code</param>
        /// <returns>List of matches</returns>
        public async Task<List<Match>> GetMatches(DateTime from, DateTime to, string sportCode)
        {
            var url = $"{Constants.WebPortalEndpoint}/MasterData/Match/{sportCode}/{Helper.GetDate(from, WebPortalDateTimeFormat)}/{Helper.GetDate(to, WebPortalDateTimeFormat)}";
            Logger.Information($"Get matches data from {url}");
            return await GetAsync<List<Match>>(url);
        }

        /// <summary>
        /// Get full matches model by date range and sport code
        /// </summary>
        /// <param name="from">From date</param>
        /// <param name="to">To date</param>
        /// <param name="sportCode">Sport code</param>
        /// <returns>List of matches</returns>
        public async Task<List<Match>> GetFullMatches(DateTime from, DateTime to, string sportCode)
        {
            var url = $"{Constants.WebPortalEndpoint}/MasterData/Match/Full/{sportCode}/{Helper.GetDate(from, WebPortalDateTimeFormat)}/{Helper.GetDate(to, WebPortalDateTimeFormat)}";
            Logger.Information($"Get matches data from {url}");
            return await GetAsync<List<Match>>(url);
        }

        /// <summary>
        /// Get all teams by sport code
        /// </summary>
        /// <param name="sportCode">Sport code</param>
        /// <returns>List of teams</returns>
        public async Task<List<Team>> GetTeams(string sportCode)
        {
            var url = $"{Constants.WebPortalEndpoint}/MasterData/Team/{sportCode}";
            Logger.Information($"Get team data from {url}");
            return await GetAsync<List<Team>>(url);
        }

        /// <summary>
        /// Create new ScrapingInformation and return that instance
        /// </summary>
        /// <returns>Created ScrapingInformation</returns>
        public async Task<ScrapingInformation> InitScrapingInformation(string providerCode, string sportCode)
        {
            var url = $"{Constants.WebPortalEndpoint}/Scraping/Init/{sportCode}/{providerCode}";
            Logger.Information($"Init scraping information data from {url}");
            return await PostAsync<ScrapingInformation>(url, null);
        }

        /// <summary>
        /// Update scraping progress
        /// </summary>
        /// <param name="scrapingInformation">To update</param>
        /// <returns>True if can update otherwise false</returns>
        public async Task<bool> UpdateScrapingProgress(ScrapingInformation scrapingInformation)
        {
            var url = $"{Constants.WebPortalEndpoint}/Scraping/UpdateScrapingInformation";
            return await PostAsync<bool>(url, scrapingInformation);
        }

        /// <summary>
        /// Insert or update matches depends on data
        /// </summary>
        /// <param name="matches">Scraped matches</param>
        public async Task<bool> InsertUpdateMatches(List<Match> matches)
        {
            var url = $"{Constants.WebPortalEndpoint}/MasterData/Match/InsertUpdate";
            Logger.Information($"Insert update matches data from {url}");
            return await PostAsync<bool>(url, matches);
        }

        /// <summary>
        /// Insert or update players depends on data
        /// </summary>
        /// <param name="players">Scraped players</param>
        public async Task<bool> InsertUpdatePlayers(List<Player> players)
        {
            var url = $"{Constants.WebPortalEndpoint}/MasterData/Player/InsertUpdate";
            Logger.Information($"Insert update players data from {url}");
            return await PostAsync<bool>(url, players);
        }

        /// <summary>
        /// Insert playerUnderOvers
        /// </summary>
        /// <param name="playerUnderOvers">Scraped playerUnderOvers</param>
        public async Task<bool> InsertPlayerUnderOvers(List<Metric> playerUnderOvers)
        {
            var url = $"{Constants.WebPortalEndpoint}/MetricData/PlayerUnderOver/Insert";
            Logger.Information($"Insert playerUnderOvers data from {url}");
            return await PostAsync<bool>(url, playerUnderOvers);
        }

        /// <summary>
        /// Insert playerHeadToHeads
        /// </summary>
        /// <param name="playerHeadToHeads">Scraped playerHeadToHeads</param>
        public async Task<bool> InsertPlayerHeadToHeads(List<Metric> playerHeadToHeads)
        {
            var url = $"{Constants.WebPortalEndpoint}/MetricData/PlayerHeadToHead/Insert";
            Logger.Information($"Insert playerHeadToHeads data from {url}");
            return await PostAsync<bool>(url, playerHeadToHeads);
        }


        /// <summary>
        /// Only fetch from web portal if not latest or null
        /// </summary>
        /// <returns>List of today matches</returns>
        public async Task<List<Match>> GetSingletonTodayMatches()
        {
            if (_todayMatches != null
                && _todayMatches
                    .TrueForAll(match => match.StartTime >= Helper.ToMinTime(DateTime.Now)
                                         && match.StartTime <= Helper.ToMaxTime(DateTime.Now)))
            {
                return _todayMatches;
            }

            _todayMatches = await GetMatches(DateTime.Now, DateTime.Now, Helper.GetSportCode());
            return _todayMatches;
        }

        /// <summary>
        /// Only fetch from web portal if null
        /// </summary>
        /// <returns></returns>
        public async Task<List<Provider>> GetSingletonProviders()
        {
            if (_providers != null)
            {
                return _providers;
            }

            _providers = await GetProviders();
            return _providers;
        }
    }
}
