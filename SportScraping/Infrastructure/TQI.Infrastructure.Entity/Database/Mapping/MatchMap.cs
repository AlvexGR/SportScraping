using Dapper.FluentMap.Mapping;
using TQI.Infrastructure.Entity.Models;

namespace TQI.Infrastructure.Entity.Database.Mapping
{
    public class MatchMap : EntityMap<Match>
    {
        public MatchMap()
        {
            Map(match => match.Id).ToColumn("id");
            Map(match => match.GameCode).ToColumn("game_code");
            Map(match => match.StartTime).ToColumn("start_time");
            Map(match => match.HomeTeamId).ToColumn("home_team_id");
            Map(match => match.AwayTeamId).ToColumn("away_team_id");
            Map(match => match.CreatedAt).ToColumn("created_at");
            Map(match => match.UpdatedAt).ToColumn("updated_at");
        }
    }
}
