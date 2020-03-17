using System.Collections.Generic;
using TQI.Infrastructure.Entity.Database.Helpers;

namespace TQI.Infrastructure.Entity.Models
{
    public class Sport : BaseModel
    {
        public string ShortName { get; set; }

        public string LongName { get; set; }

        public string Code { get; set; }

        [IgnoreProperty]
        public List<Provider> Providers { get; set; }
    }
}
