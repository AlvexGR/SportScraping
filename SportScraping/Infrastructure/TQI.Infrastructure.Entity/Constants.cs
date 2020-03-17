namespace TQI.Infrastructure.Entity
{
    public class Constants
    {
        public const string AppSetting = "appsettings.json";
        public const string SportCodeKey = "SportCode";
        public const string ProvidersKey = "Providers";
        public const string ContractNameKey = "WcfContractName";

        public const string BaseLoggerPath = @"C:\SportScraping\Logs";

        /// <summary>
        /// Scraping hours before the first match starts
        /// </summary>
        public const int ScrapingMetricHourBefore = 5;

        /// <summary>
        /// Retry attempt for scraping failed
        /// </summary>
        public const int RetryAttempt = 5;

        /// <summary>
        /// Retry scraping after some milliseconds
        /// </summary>
        public const int RetryAfter = 5000; // 5 seconds

#if DEBUG
        //public const string WebPortalEndpoint = "https://localhost:44326/api"; // Local
        //public const string WebPortalEndpoint = "http://192.168.1.10:1503/api"; // Home
        //public const string WebPortalEndpoint = "http://localhost:1503/api"; // IMT
        public const string WebPortalEndpoint = "http://40.115.70.210:1503/api"; // server
#else
        public const string WebPortalEndpoint = "http://40.115.70.210:1503/api";
#endif
    }

    /// <summary>
    /// Score type for player under over data
    /// </summary>
    public class ScoreType
    {
        public const string Point = "P";
        public const string Assist = "A";
        public const string Rebound = "R";
        public const string ThreePoint = "3P";
        public const string PointAssist = "PA";
        public const string PointRebound = "PR";
        public const string ReboundAssist = "RA";
        public const string PointReboundAssist = "PRA";
    }
}
