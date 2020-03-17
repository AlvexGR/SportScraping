using System.Collections.Generic;
using System.Text;

namespace TQI.Infrastructure.Utility
{
    /// <summary>
    /// Cron builder to create cron expression
    /// </summary>
    public class CronBuilder
    {
        private string _seconds = "*";
        private string _minutes = "*";
        private string _hours = "*";
        private string _dayOfMonth = "*";
        private string _month = "*";
        private string _dayOfWeek = "?";
        private string _year = "*";

        #region Seconds

        public CronBuilder AtSpecificSecond(int second)
        {
            _seconds = second.ToString();
            return this;
        }

        public CronBuilder AtSpecificSecond(List<int> seconds)
        {
            var sb = new StringBuilder();
            seconds.ForEach(second =>
            {
                if (!string.IsNullOrEmpty(sb.ToString()))
                {
                    sb.Append(",");
                }

                sb.Append(second);
            });
            _seconds = sb.ToString();
            return this;
        }

        #endregion

        #region Minutes

        public CronBuilder EveryMinutesStartingAt(int everyMinutes, int startAt)
        {
            _minutes = $"{startAt}/{everyMinutes}";
            return this;
        }

        #endregion

        #region Hours

        public CronBuilder EveryHourBetween(int fromHour, int toHour)
        {
            _hours = $"{fromHour}-{toHour}";
            return this;
        }

        #endregion

        #region Day of month

        public CronBuilder AtSpecificDayOfMonth(int day)
        {
            _dayOfMonth = day.ToString();
            return this;
        }

        #endregion

        #region Months

        public CronBuilder AtSpecificMonth(int month)
        {
            _month = month.ToString();
            return this;
        }

        #endregion

        #region Day of week

        #endregion

        #region Years

        public CronBuilder AtSpecificYear(int year)
        {
            _year = year.ToString();
            return this;
        }

        #endregion

        public string Build()
        {
            return $"{_seconds} {_minutes} {_hours} {_dayOfMonth} {_month} {_dayOfWeek} {_year}";
        }
    }
}
