using Dapper.FluentMap.Mapping;
using TQI.Infrastructure.Entity.Models;

namespace TQI.Infrastructure.Entity.Database.Mapping
{
    public class PlayerMap : EntityMap<Player>
    {
        public PlayerMap()
        {
            Map(player => player.Id).ToColumn("id");
            Map(player => player.TeamId).ToColumn("team_id");
            Map(player => player.Name).ToColumn("name");
            Map(player => player.SourceId).ToColumn("source_id");
            Map(player => player.CreatedAt).ToColumn("created_at");
            Map(player => player.UpdatedAt).ToColumn("updated_at");
        }
    }
}
