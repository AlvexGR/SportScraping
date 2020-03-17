using Dapper.FluentMap.Mapping;
using TQI.Infrastructure.Entity.Models;

namespace TQI.Infrastructure.Entity.Database.Mapping
{
    public class TempTableToTestMap : EntityMap<TempTableToTest>
    {
        public TempTableToTestMap()
        {
            Map(item => item.Id).ToColumn("id");
            Map(item => item.Name).ToColumn("name");
            Map(item => item.Description).ToColumn("description");
        }
    }
}
