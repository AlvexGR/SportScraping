using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Models;
using TQI.Infrastructure.Entity.Models.Metrics;
using TQI.Infrastructure.Scrape.Handler;
using TQI.Scrape.NBA.Handler.Handlers.Masters;
using TQI.Scrape.NBA.Handler.Handlers.Metrics.PlayerHeadToHeads;
using TQI.Scrape.NBA.Handler.Handlers.Metrics.PlayerOverUnders;

namespace TQI.Runner.Debugger
{
    internal class Program
    {
        private static async Task Main()
        {
            //Create scrape handler instance to run
            var provider = await ScrapeHandlerFactory.CreateAsync(typeof(TopSportPlayerOverUnder));
            //var provider = ScrapeHandlerFactory.Create<EspnCompetition>();
            await provider.Scrape();
        }
    }
}