using System.Collections.Specialized;
using System.ServiceModel;
using System.ServiceProcess;
using Quartz.Impl;
using Serilog;

namespace TQI.Infrastructure.Scrape.Service
{
    public abstract class BaseScrapingService : ServiceBase
    {
        public static readonly StdSchedulerFactory SchedulerFactory =
            new StdSchedulerFactory(new NameValueCollection
            {
                { "quartz.serializer.type", "binary" }
            });
        private ServiceHost _serviceHost;
        protected ILogger Logger;

        protected override void OnStart(string[] args)
        {
#if !DEBUG
            Logger.Information("Start WCF hosting");
            _serviceHost.Open();
#endif
        }

        protected override void OnStop()
        {
#if !DEBUG
            Logger.Information("Close WCF hosting");
            _serviceHost?.Close();
            _serviceHost = null;
#endif
        }

        /// <summary>
        /// Create service host
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected void CreateServiceHost<T>()
        {
            Logger.Information("Init WCF service host");
            _serviceHost?.Close();
            _serviceHost = new ServiceHost(typeof(T));
        }

#if DEBUG
        /// <summary>
        /// For debugging the service
        /// </summary>
        protected void RunDebug()
        {
            OnStart(null);
        }
#endif
    }
}
