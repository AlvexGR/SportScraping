namespace TQI.Infrastructure.Entity.Models
{
    public class Provider : BaseModel
    {
        public string Name { get; set; }

        /// <summary>
        /// To parse class name
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Scrape data for this provider or not
        /// </summary>
        public bool? DoScrape { get; set; }

        /// <summary>
        /// Determine if provider is for metric or master
        /// </summary>
        public bool IsMetric { get; set; }

        public ScrapeType ScrapeType { get; set; }

        public string CountryCode { get; set; }

        public string Url { get; set; }

        public string SportCode { get; set; }
    }
}
