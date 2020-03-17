using System;
using TQI.Infrastructure.Entity.Database.Helpers;

namespace TQI.Infrastructure.Entity.Models
{
    public class Match : BaseModel
    {
        public string GameCode { get; set; }

        public DateTime StartTime { get; set; }

        public string SportCode { get; set; }

        public int HomeTeamId { get; set; }

        [IgnoreProperty]
        public Team HomeTeam { get; set; }

        public int AwayTeamId { get; set; }

        [IgnoreProperty]
        public Team AwayTeam { get; set; }

        /// <summary>
        /// For specific provider webpage source id
        /// </summary>
        [IgnoreProperty]
        public string SourceId { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
