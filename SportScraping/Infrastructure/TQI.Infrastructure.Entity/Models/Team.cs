using System.Collections.Generic;
using TQI.Infrastructure.Entity.Database.Helpers;

namespace TQI.Infrastructure.Entity.Models
{
    public class Team : BaseModel
    {
        public string LongName { get; set; }

        public string ShortName { get; set; }

        [IgnoreProperty]
        public List<Player> Players { get; set; }

        public string SportCode { get; set; }
    }
}
