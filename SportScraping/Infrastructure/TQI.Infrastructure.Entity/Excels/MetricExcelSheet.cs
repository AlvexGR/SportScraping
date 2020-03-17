using System.Collections.Generic;

namespace TQI.Infrastructure.Entity.Excels
{
    public class MetricExcelSheet
    {
        public string Name { get; set; }

        public List<string> ColumnNames { get; set; }

        public List<List<string>> Data { get; set; }
    }
}
