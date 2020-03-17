using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TQI.Infrastructure.Entity.Models;

namespace TQI.Infrastructure.Entity.Database.Helpers
{
    public class DbConverter<TEntity> where TEntity : BaseModel
    {
        /// <summary>
        /// Convert class TableInDatabase to table_in_database
        /// </summary>
        /// <returns>Converted table name</returns>
        public static string ToTableName()
        {
            return $"`sports_scraping`.`{ToSnareCase(typeof(TEntity).Name)}`";
        }

        /// <summary>
        /// Make list of properties to format (col1, col2, col3,...)
        /// </summary>
        /// <param name="ignoreId">Should ignore Id param or not</param>
        /// <returns>Formatted columns string</returns>
        public static string ToColumnNames(bool ignoreId = true)
        {
            var properties = typeof(TEntity)
                .GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(IgnorePropertyAttribute), true).Length == 0)
                .ToList();

            var builder = new StringBuilder();
            foreach (var property in properties.Where(property => !ignoreId || property.Name != "Id"))
            {
                builder.Append($"{ToSnareCase(property.Name)}, ");
            }
            // Remove last space and comma
            builder.Remove(builder.Length - 2, 2);

            return builder.ToString();
        }

        /// <summary>
        /// Make list of properties to format (@col1, @col2, @col3,...)
        /// </summary>
        /// <param name="ignoreId">Should ignore Id param or not</param>
        /// <returns>Formatted parameters string</returns>
        public static string ToParameterNames(bool ignoreId = true)
        {
            var properties = typeof(TEntity)
                .GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(IgnorePropertyAttribute), true).Length == 0)
                .ToList();

            var builder = new StringBuilder();
            foreach (var property in properties.Where(property => !ignoreId || property.Name != "Id"))
            {
                builder.Append($"@{property.Name}, ");
            }
            // Remove last space and comma
            builder.Remove(builder.Length - 2, 2);

            return builder.ToString();
        }

        /// <summary>
        /// Only work for standard Pascal Case
        /// </summary>
        /// <param name="toConvert">Input string to convert</param>
        /// <returns>To lower snare case</returns>
        private static string ToSnareCase(string toConvert)
        {
            if (string.IsNullOrEmpty(toConvert))
            {
                throw new ArgumentException("toConvert is null or empty");
            }
            if (toConvert.Length == 1)
            {
                return toConvert.ToLower();
            }
            var words = new List<string>();
            var builder = new StringBuilder();
            builder.Append(toConvert[0]);
            for (var index = 1; index < toConvert.Length; index++)
            {
                if (char.IsUpper(toConvert[index]))
                {
                    words.Add(builder.ToString());
                    builder = new StringBuilder();
                }
                builder.Append(toConvert[index]);
            }
            words.Add(builder.ToString());

            builder = new StringBuilder(words[0]);
            for (var index = 1; index < words.Count; index++)
            {
                builder.Append($"_{words[index]}");
            }

            return builder.ToString().ToLower();
        }
    }
}
