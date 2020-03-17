using TQI.Infrastructure.Entity.Database.Helpers;

namespace TQI.Infrastructure.Entity.Models.Metrics
{
    public class PlayerHeadToHead : Metric
    {
        public int PlayerAId { get; set; }

        [IgnoreProperty]
        public Player PlayerA { get; set; }

        public double? PlayerAPrice { get; set; }

        public int PlayerBId { get; set; }

        [IgnoreProperty]
        public Player PlayerB { get; set; }

        public double? PlayerBPrice { get; set; }

        public bool IsTieIncluded { get; set; }

        public double? TiePrice { get; set; }
    }
}
