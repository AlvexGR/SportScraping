using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Utility;

namespace TQI.WebPortal.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog(Helper
                    .GetLoggerConfig($@"{Constants.BaseLoggerPath}\WebPortal\webportal-.txt")
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                    .CreateLogger());
    }
}
