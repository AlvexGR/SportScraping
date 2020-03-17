using System.ServiceModel;

namespace TQI.Scrape.NBA.ServiceScheduler.WcfContract
{
    /// <summary>
    /// Service contract for NBA
    /// </summary>
    [ServiceContract(Namespace = "http://TQI.Scrape.NBA.ServiceScheduler")]
    public interface INBAScrapingContract
    {
        /// <summary>
        /// Get current progress of a provider
        /// </summary>
        /// <param name="providerCode">Provider name</param>
        /// <returns></returns>
        [OperationContract]
        int GetProgress(string providerCode);
    }
}
