using Dapper.FluentMap.Mapping;
using TQI.Infrastructure.Entity.Models.Metrics;

namespace TQI.Infrastructure.Entity.Database.Mapping.Metrics
{
    public class MetricMap : EntityMap<Metric>
    {
        public MetricMap()
        {
            Map(metric => metric.Id).ToColumn("id");
            Map(metric => metric.MatchId).ToColumn("match_id");
            Map(metric => metric.ScrapingInformationId).ToColumn("scraping_information_id");
            Map(metric => metric.CreatedAt).ToColumn("created_at");
        }
    }
}
