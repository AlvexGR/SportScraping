using Dapper.FluentMap.Mapping;
using TQI.Infrastructure.Entity.Models;

namespace TQI.Infrastructure.Entity.Database.Mapping
{
    public class SportMap : EntityMap<Sport>
    {
        public SportMap()
        {
            Map(sport => sport.Id).ToColumn("id");
            Map(sport => sport.LongName).ToColumn("long_name");
            Map(sport => sport.ShortName).ToColumn("short_name");
            Map(sport => sport.Code).ToColumn("code");
        }
    }
}
