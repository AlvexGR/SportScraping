using System;
using TQI.Infrastructure.Entity.Database.Helpers;

namespace TQI.Infrastructure.Entity.Models
{
    public class Player : BaseModel
    {
        public string SourceId { get; set; }

        public string Name { get; set; }

        public int? TeamId { get; set; }

        [IgnoreProperty]
        public Team Team { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
