using TQI.Infrastructure.Entity.Database.Helpers;

namespace TQI.Infrastructure.Entity.Models.Metrics
{
    public class PlayerOverUnder : Metric
    {
        public int PlayerId { get; set; }

        [IgnoreProperty]
        public Player Player { get; set; }

        public string ScoreType { get; set; }

        /// <summary>
        /// Under value
        /// </summary>
        public double? UnderLine { get; set; }

        /// <summary>
        /// Over value
        /// </summary>
        public double? OverLine { get; set; }

        public double? Over { get; set; }

        public double? Under { get; set; }
    }
}
