using System;
using Quartz;
using Quartz.Spi;

namespace TQI.Infrastructure.Scrape.Scheduler
{
    /// <summary>
    /// For dependency injection
    /// </summary>
    public class ScrapeJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ScrapeJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return _serviceProvider.GetService(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job)
        {
            var disposable = job as IDisposable;
            disposable?.Dispose();
        }
    }
}
