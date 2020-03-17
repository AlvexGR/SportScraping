using Dapper.FluentMap.Mapping;
using TQI.Infrastructure.Entity.Models;

namespace TQI.Infrastructure.Entity.Database.Mapping
{
    public class TeamMap : EntityMap<Team>
    {
        public TeamMap()
        {
            Map(team => team.Id).ToColumn("id");
            Map(team => team.LongName).ToColumn("long_name");
            Map(team => team.ShortName).ToColumn("short_name");
            Map(team => team.SportCode).ToColumn("sport_code");
        }
    }
}
