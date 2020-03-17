using System;
using TQI.Infrastructure.Entity.Database.Helpers;

namespace TQI.Infrastructure.Entity.Models
{
    public class ScrapingInformation : BaseModel
    {
        public int Progress { get; set; }

        public string ProgressExplanation { get; set; }

        public ScrapeStatus ScrapeStatus { get; set; }

        public int ProviderId { get; set; }

        [IgnoreProperty]
        public Provider Provider { get; set; }

        public DateTime ScrapeTime { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
