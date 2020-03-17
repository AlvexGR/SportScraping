namespace TQI.Scrape.NBA.ServiceScheduler.WcfContract
{
    public class NBAScrapingContract : INBAScrapingContract
    {
        public int GetProgress(string providerCode)
        {
            return 50;
        }
    }
}
