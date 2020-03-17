using Dapper.FluentMap.Mapping;
using TQI.Infrastructure.Entity.Models;

namespace TQI.Infrastructure.Entity.Database.Mapping
{
    public class ScrapingInformationMap : EntityMap<ScrapingInformation>
    {
        public ScrapingInformationMap()
        {
            Map(scrapingInfo => scrapingInfo.Id).ToColumn("id");
            Map(scrapingInfo => scrapingInfo.Progress).ToColumn("progress");
            Map(scrapingInfo => scrapingInfo.ProgressExplanation).ToColumn("progress_explanation");
            Map(scrapingInfo => scrapingInfo.ProviderId).ToColumn("provider_id");
            Map(scrapingInfo => scrapingInfo.ScrapeStatus).ToColumn("scrape_status");
            Map(scrapingInfo => scrapingInfo.ScrapeTime).ToColumn("scrape_time");
            Map(scrapingInfo => scrapingInfo.CreatedAt).ToColumn("created_at");
            Map(scrapingInfo => scrapingInfo.UpdatedAt).ToColumn("updated_at");
        }
    }
}
