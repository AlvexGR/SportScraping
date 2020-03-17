using System;
using TQI.Infrastructure.Entity.Database.Helpers;

namespace TQI.Infrastructure.Entity.Models.Metrics
{
    public class Metric : BaseModel
    {
        public int MatchId { get; set; }

        [IgnoreProperty]
        public Match Match { get; set; }

        public int ScrapingInformationId { get; set; }

        [IgnoreProperty]
        public ScrapingInformation ScrapingInformation { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
