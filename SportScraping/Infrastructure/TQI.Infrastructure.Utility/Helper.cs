using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using TQI.Infrastructure.Entity;
using TQI.Infrastructure.Entity.Models;

namespace TQI.Infrastructure.Utility
{
    public static class Helper
    {
        /// <summary>
        /// Get only DoScrape == true from settings
        /// </summary>
        /// <returns>List of active providers</returns>
        public static List<Provider> GetActiveProviders()
        {
            var file = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/{Constants.AppSetting}");
            var providers = new List<Provider>();
            if (!string.IsNullOrEmpty(file))
            {
                providers = JsonConvert
                    .DeserializeObject<JToken>(file)
                    .SelectToken($"$.{Constants.ProvidersKey}")
                    .ToObject<List<Provider>>()
                    .Where(x => x.DoScrape == true)
                    .ToList();
            }
            return providers;
        }

        /// <summary>
        /// Get sport code from settings
        /// </summary>
        /// <returns>Sport name</returns>
        public static string GetSportCode()
        {
            var file = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/{Constants.AppSetting}");
            var sportId = string.Empty;
            if (!string.IsNullOrEmpty(file))
            {
                sportId = JsonConvert
                    .DeserializeObject<JToken>(file)
                    .SelectToken($"$.{Constants.SportCodeKey}")
                    .Value<string>();
            }
            return sportId;
        }

        /// <summary>
        /// Get wcf contract
        /// </summary>
        /// <returns>Wcf contract name</returns>
        public static string GetWcfContract()
        {
            var file = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/{Constants.AppSetting}");
            var wcfName = string.Empty;
            if (!string.IsNullOrEmpty(file))
            {
                wcfName = JsonConvert
                    .DeserializeObject<JToken>(file)
                    .SelectToken($"$.{Constants.ContractNameKey}")
                    .Value<string>();
            }
            // Need to name exactly the same to work
            return $"TQI.Infrastructure.Scrape.WcfContract.{GetSportCode()}.{wcfName}";
        }

        /// <summary>
        /// Get current date with format
        /// </summary>
        /// <param name="format">Input format with default "yyyyMMdd"</param>
        /// <returns>Date as formatted string</returns>
        public static string GetCurrentDate(string format = "yyyyMMdd")
        {
            return DateTime.Now.ToString(format);
            //return DateTime.Now.AddDays(1).ToString(format);
        }

        /// <summary>
        /// Get by a given date with format 
        /// </summary>
        /// <param name="dateTime">Input datetime to parse</param>
        /// <param name="format">Input format with default "yyyyMMdd"</param>
        /// <returns>Date as formatted string</returns>
        public static string GetDate(DateTime dateTime, string format = "yyyyMMdd")
        {
            return dateTime.ToString(format);
        }

        /// <summary>
        /// Convert time to 00:00:00
        /// </summary>
        /// <param name="dateTime">Datetime to convert</param>
        /// <returns>Datetime with time 00:00:00</returns>
        public static DateTime ToMinTime(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
        }

        /// <summary>
        /// Convert time to 23:59:59
        /// </summary>
        /// <param name="dateTime">Datetime to convert</param>
        /// <returns>Datetime with time 23:59:59</returns>
        public static DateTime ToMaxTime(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59);
        }

        /// <summary>
        /// Unique Job key for providerCode
        /// </summary>
        /// <param name="providerCode">Provider code</param>
        /// <returns>Job key</returns>
        public static string GetJobKey(string providerCode)
        {
            return $"{providerCode}.Job";
        }

        /// <summary>
        /// Unique Trigger key for providerCode
        /// </summary>
        /// <param name="providerCode">Provider code</param>
        /// <returns>Trigger key</returns>
        public static string GetTriggerKey(string providerCode)
        {
            return $"{providerCode}.Trigger";
        }

        /// <summary>
        /// Get base logger configuration with basic configs
        /// </summary>
        /// <param name="filePath">Location to save log file</param>
        /// <returns>LoggerConfiguration</returns>
        public static LoggerConfiguration GetLoggerConfig(string filePath)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.File(filePath
                    , rollingInterval: RollingInterval.Day
                    , outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message} {RequestId}{NewLine}{Exception}");
        }
    }
}
