using System;
using System.Text;

namespace TQI.Infrastructure.Entity.Database.Helpers
{
    public class QueryGenerator
    {
        /// <summary>
        /// Create insert query
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnNames">List of columnNames as string</param>
        /// <param name="parameterNames">List of parameters as string</param>
        /// <returns>Formatted insert query string</returns>
        public static string GenerateInsertQuery(string tableName, string columnNames, string parameterNames)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("TableName is null or empty");
            }
            if (string.IsNullOrEmpty(columnNames))
            {
                throw new ArgumentException("ColumnNames is null or empty");
            }
            if (string.IsNullOrEmpty(parameterNames))
            {
                throw new ArgumentException("ParameterNames is null or empty");
            }
            return $"INSERT INTO {tableName} ({columnNames}) VALUES ({parameterNames});";
        }

        /// <summary>
        /// Create update query
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnNames">List of columnNames as string</param>
        /// <param name="parameterNames">List of parameters as string</param>
        /// <param name="condition">Condition to update</param>
        /// <returns>Formatted update query string</returns>
        public static string GenerateUpdateQuery(string tableName, string columnNames, string parameterNames, string condition)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("TableName is null or empty");
            }
            if (string.IsNullOrEmpty(columnNames))
            {
                throw new ArgumentException("ColumnNames is null or empty");
            }
            if (string.IsNullOrEmpty(parameterNames))
            {
                throw new ArgumentException("ParameterNames is null or empty");
            }
            if (string.IsNullOrEmpty(condition))
            {
                throw new ArgumentException("Condition is null or empty");
            }

            var splitColNames = columnNames.Split(',');
            var splitParamNames = parameterNames.Split(',');
            if (splitColNames.Length != splitParamNames.Length)
            {
                throw new InvalidOperationException(
                    $"Mismatch length between columnsNames({columnNames.Length}) " +
                    $"and parameterNames({parameterNames.Length})");
            }
            var builder = new StringBuilder();
            builder.Append($"UPDATE {tableName} SET ");
            for (var i = 0; i < splitColNames.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }
                builder.AppendFormat($"{splitColNames[i].Trim()} = {splitParamNames[i].Trim()}");
            }
            builder.Append($" WHERE ({condition});").AppendLine();
            return builder.ToString();
        }

        /// <summary>
        /// Create delete query
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="condition">Condition to delete. Default: id != 0, means to delete everything</param>
        /// <returns>Formatted delete query string</returns>
        public static string GenerateDeleteQuery(string tableName, string condition)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("TableName is null or empty");
            }
            return $"DELETE FROM {tableName} WHERE ({(!string.IsNullOrEmpty(condition) ? condition : "id != 0")});";
        }
    }
}
