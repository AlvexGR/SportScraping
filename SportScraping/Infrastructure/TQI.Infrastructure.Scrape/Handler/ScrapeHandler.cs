using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Entity.Models;
using TQI.Infrastructure.Entity.Models.Metrics;
using TQI.Infrastructure.Utility;

namespace TQI.Infrastructure.Scrape.Handler
{
    /// <summary>
    /// Scrape data handler
    /// </summary>
    public abstract class ScrapeHandler : IScrapeHandler
    {
        public bool ShouldGetTodayMatches { get; set; } = true;

        private ScrapingInformation _scrapeInformation;

        protected readonly ILogger Logger;
        protected readonly AppEnvironment Environment;
        protected readonly WebPortalHelper WebPortalHelper;
        protected readonly ScrapeHelper ScrapeHelper;

        // Required data
        protected List<Match> TodayMatches = new List<Match>();

        // Data container
        protected List<Match> Matches = new List<Match>();
        protected List<Player> Players = new List<Player>();
        protected List<Metric> PlayerUnderOvers = new List<Metric>();
        protected List<Metric> PlayerHeadToHeads = new List<Metric>();

        protected ScrapeHandler(ILogger logger, WebPortalHelper webPortalHelper, ScrapeHelper scrapeHelper)
        {
            Logger = logger;
            Environment = AppEnvironment.Production;
            WebPortalHelper = webPortalHelper;
            ScrapeHelper = scrapeHelper;

            // Inject
            WebPortalHelper.Logger = ScrapeHelper.Logger = Logger;
        }

        /// <summary>
        /// For debugging purpose only
        /// </summary>
        protected ScrapeHandler()
        {
            Logger = Helper
                .GetLoggerConfig($@"{Constants.BaseLoggerPath}\Debug\debug-.txt")
                .CreateLogger();
            Environment = AppEnvironment.Debug;
            WebPortalHelper = new WebPortalHelper(new HttpClient());
            ScrapeHelper = new ScrapeHelper(new HttpClient(), new WebClient());
            //ShouldGetTodayMatches = false;

            // Inject
            WebPortalHelper.Logger = ScrapeHelper.Logger = Logger;
        }

        public async Task Initialize(Type providerType)
        {
            await InitScrapingInformation(providerType.Name);
        }

        public async Task<bool> Scrape()
        {
            var stopwatch = Stopwatch.StartNew();
            bool result;
            Logger.Information("==========Start scraping==========");
            try
            {
                // Template design pattern

                await UpdateScrapeStatus(0, "Start scraping", ScrapeStatus.InProgress);

                await InitializeData();

                if (ShouldGetTodayMatches && (TodayMatches == null || TodayMatches.Count == 0))
                {
                    Logger.Information("No match for today");
                    await UpdateScrapeStatus(100, "No match for today", ScrapeStatus.Done);
                    return false;
                }

                var scrapeDataStopwatch = Stopwatch.StartNew();
                await ScrapeData();
                scrapeDataStopwatch.Stop();
                Logger.Information($"Scrape data execution time: {scrapeDataStopwatch.Elapsed.TotalMilliseconds}ms");

                await UpdateScrapeStatus(90, "Send data to web portal", null);

                //if (Environment == AppEnvironment.Debug) return false;
                var saveDataStopwatch = Stopwatch.StartNew();
                await SaveData();
                saveDataStopwatch.Stop();
                Logger.Information($"Save data execution time: {saveDataStopwatch.Elapsed.TotalMilliseconds}ms");

                await UpdateScrapeStatus(100, "Scrape complete", ScrapeStatus.Done);

                result = true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Scrape error: {ex}");
                await UpdateScrapeStatus(null, "Scrape failed", ScrapeStatus.Failed);
                result = false;
            }
            stopwatch.Stop();
            Logger.Information($"Total execution time: {stopwatch.Elapsed.TotalMilliseconds}ms");
            Logger.Information("==========Scrape completed==========");
            return result;
        }

        /// <summary>
        /// Clean all data before start scraping
        /// </summary>
        private async Task InitializeData()
        {
            Matches.Clear();
            Players.Clear();
            PlayerHeadToHeads.Clear();
            PlayerUnderOvers.Clear();

            if (!ShouldGetTodayMatches) return;
            //TodayMatches = await WebPortalHelper.GetFullMatches(DateTime.Now, DateTime.Now, Helper.GetSportCode());
            TodayMatches = await WebPortalHelper.GetFullMatches(DateTime.Now.AddDays(1), DateTime.Now.AddDays(1), Helper.GetSportCode());
        }

        /// <summary>
        /// Scrape data from provider
        /// </summary>
        /// <returns>Raw data</returns>
        protected abstract Task ScrapeData();

        /// <summary>
        /// Send processed data to web portal
        /// This method will update progress from 90% -> 100%
        /// </summary>
        private async Task SaveData()
        {
            if (Matches.Count == 0
                && Players.Count == 0
                && PlayerUnderOvers.Count == 0
                && PlayerHeadToHeads.Count == 0)
            {
                Logger.Warning("No data to send");
                return;
            }

            var tasks = new List<Task>();

            if (Matches.Count > 0)
            {
                Logger.Information("Send Matches");
                tasks.Add(WebPortalHelper.InsertUpdateMatches(Matches));
            }

            if (Players.Count > 0)
            {
                Logger.Information("Send Players");
                tasks.Add(WebPortalHelper.InsertUpdatePlayers(Players));
            }

            if (PlayerUnderOvers.Count > 0)
            {
                Logger.Information("Send PlayerUnderOvers");
                tasks.Add(WebPortalHelper.InsertPlayerUnderOvers(PlayerUnderOvers));
            }

            if (PlayerHeadToHeads.Count > 0)
            {
                Logger.Information("Send PlayerHeadToHeads");
                tasks.Add(WebPortalHelper.InsertPlayerHeadToHeads(PlayerHeadToHeads));
            }

            await Task.WhenAll(tasks);
            Logger.Information("Send data complete");
        }

        #region Handle ScrapeInformation

        private async Task InitScrapingInformation(string providerCode)
        {
            _scrapeInformation = await WebPortalHelper.InitScrapingInformation(providerCode, Helper.GetSportCode());
            Logger.Information($"Scrape information: Id {_scrapeInformation.Id} - Provider {_scrapeInformation.ProviderId}");
        }

        public ScrapingInformation GetScrapingInformation()
        {
            if (_scrapeInformation != null) return _scrapeInformation;
            Logger.Warning("GetScrapingInformation failed: ScrapingInformation is null");
            return null;
        }

        /// <summary>
        /// Update progress. NOTE: newProgress must be in range from 0 to 90
        /// </summary>
        /// <param name="newProgress">New progress to update. (0 -> 90)</param>
        /// <param name="explanation">Explanation to new progress</param>
        protected async Task UpdateScrapeStatus(int? newProgress, string explanation)
        {
            if (_scrapeInformation == null)
            {
                Logger.Warning("SetProgress failed: ScrapingInformation is null");
                return;
            }

            if (newProgress != null)
            {
                if (newProgress < 0 || newProgress > 90)
                {
                    throw new InvalidOperationException("Progress must in range from 0 to 90 percent");
                }
                _scrapeInformation.Progress = (int)newProgress;
            }

            if (!string.IsNullOrEmpty(explanation))
            {
                _scrapeInformation.ProgressExplanation = explanation;
            }

            _scrapeInformation.UpdatedAt = DateTime.Now;
            var result = await WebPortalHelper.UpdateScrapingProgress(_scrapeInformation);
            if (!result)
            {
                Logger.Warning("Cannot update scraping information");
            }
        }

        private async Task UpdateScrapeStatus(int? newProgress, string explanation, ScrapeStatus? newScrapeStatus)
        {
            if (_scrapeInformation == null)
            {
                Logger.Warning("SetProgress failed: ScrapingInformation is null");
                return;
            }

            if (newProgress != null)
            {
                _scrapeInformation.Progress = (int)newProgress;
            }

            if (!string.IsNullOrEmpty(explanation))
            {
                _scrapeInformation.ProgressExplanation = explanation;
            }

            if (newScrapeStatus != null)
            {
                _scrapeInformation.ScrapeStatus = (ScrapeStatus) newScrapeStatus;
            }

            _scrapeInformation.UpdatedAt = DateTime.Now;
            var result = await WebPortalHelper.UpdateScrapingProgress(_scrapeInformation);
            if (!result)
            {
                Logger.Warning("Cannot update scraping information");
            }
        }

        #endregion
    }
}
