using Dapper.FluentMap.Mapping;
using TQI.Infrastructure.Entity.Models.Metrics;

namespace TQI.Infrastructure.Entity.Database.Mapping.Metrics
{
    public class PlayerOverUnderMap : EntityMap<PlayerOverUnder>
    {
        public PlayerOverUnderMap()
        {
            Map(overUnder => overUnder.Id).ToColumn("metric_id");
            Map(overUnder => overUnder.PlayerId).ToColumn("player_id");
            Map(overUnder => overUnder.ScoreType).ToColumn("score_type");
            Map(overUnder => overUnder.Over).ToColumn("over");
            Map(overUnder => overUnder.OverLine).ToColumn("over_line");
            Map(overUnder => overUnder.Under).ToColumn("under");
            Map(overUnder => overUnder.UnderLine).ToColumn("under_line");
        }
    }
}
