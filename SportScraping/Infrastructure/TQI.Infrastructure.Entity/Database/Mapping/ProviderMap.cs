using Dapper.FluentMap.Mapping;
using TQI.Infrastructure.Entity.Models;

namespace TQI.Infrastructure.Entity.Database.Mapping
{
    public class ProviderMap : EntityMap<Provider>
    {
        public ProviderMap()
        {
            Map(provider => provider.Id).ToColumn("id");
            Map(provider => provider.Code).ToColumn("code");
            Map(provider => provider.Name).ToColumn("name");
            Map(provider => provider.CountryCode).ToColumn("country_code");
            Map(provider => provider.IsMetric).ToColumn("is_metric");
            Map(provider => provider.Url).ToColumn("url");
            Map(provider => provider.SportCode).ToColumn("sport_code");
            Map(provider => provider.ScrapeType).ToColumn("scrape_type");
        }
    }
}
