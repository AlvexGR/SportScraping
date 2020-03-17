using Dapper.FluentMap.Mapping;
using TQI.Infrastructure.Entity.Models.Metrics;

namespace TQI.Infrastructure.Entity.Database.Mapping.Metrics
{
    public class PlayerHeadToHeadMap : EntityMap<PlayerHeadToHead>
    {
        public PlayerHeadToHeadMap()
        {
            Map(headToHead => headToHead.Id).ToColumn("metric_id");
            Map(headToHead => headToHead.PlayerAId).ToColumn("player_a_id");
            Map(headToHead => headToHead.PlayerBId).ToColumn("player_b_id");
            Map(headToHead => headToHead.PlayerAPrice).ToColumn("player_a_price");
            Map(headToHead => headToHead.PlayerBPrice).ToColumn("player_b_price");
            Map(headToHead => headToHead.IsTieIncluded).ToColumn("is_tie_included");
            Map(headToHead => headToHead.TiePrice).ToColumn("tie_price");
        }
    }
}
