using System;

namespace TQI.Infrastructure.Entity.Database
{
    public class DbConnectionString : IDisposable
    {
        public string ConnectionString { get; set; }

        public DbConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public void Dispose()
        {
            ConnectionString = string.Empty;
        }
    }
}
