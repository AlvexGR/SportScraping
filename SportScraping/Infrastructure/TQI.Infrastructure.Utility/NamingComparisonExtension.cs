using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TQI.Infrastructure.Utility
{
    public static class NamingComparisonExtension
    {
        /// <summary>
        /// Naming comparison logic
        /// </summary>
        public static bool CompareName(this string source, string toCompare)
        {
            if (string.IsNullOrEmpty(source)
                || string.IsNullOrWhiteSpace(source)
                || string.IsNullOrEmpty(toCompare)
                || string.IsNullOrWhiteSpace(toCompare)) return false;

            source = source.RemoveSpecialCharacters();
            toCompare = toCompare.RemoveSpecialCharacters();

            var sources = Regex.Split(source.ToLower(), " ");
            var toCompares = Regex.Split(toCompare.ToLower(), " ");

            var sourceNames = GetListOfNonEmptyNames(sources);
            var toCompareNames = GetListOfNonEmptyNames(toCompares);

            if (sourceNames.Count == 0 || toCompareNames.Count == 0) return false;

            var index = toCompareNames.Count;
            var equalNames = 0;
            if (toCompareNames.Count == sourceNames.Count)
            {
                var count = 0;
                foreach (var toCompareName in toCompareNames)
                {
                    foreach (var sourceName in sourceNames)
                    {
                        if (string.Equals(toCompareName, sourceName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            equalNames++;
                        }

                        if (toCompareName.Contains(sourceName) || sourceName.Contains(toCompareName))
                        {
                            count++;
                        }
                    }
                }
                if (equalNames >= 1 && count >= index)
                {
                    return true;
                }
            }
            if (sourceNames.Count == toCompareNames.Count) return false;

            index = Math.Min(sourceNames.Count, toCompareNames.Count);

            equalNames = toCompareNames
                .Sum(toCompareName => sourceNames
                    .Count(sourceName => string.Equals(toCompareName, sourceName, StringComparison.CurrentCultureIgnoreCase)));

            if (index == 1 && toCompareNames.Last().Contains(sourceNames.Last()) && equalNames > 1)
            {
                return true;
            }

            return toCompareNames.First().Contains(sourceNames.First())
                   && toCompareNames.Last().Contains(sourceNames.Last())
                   && equalNames > 1;
        }

        private static List<string> GetListOfNonEmptyNames(IEnumerable<string> names)
        {
            return names.Where(name => !string.IsNullOrEmpty(name) | !string.IsNullOrWhiteSpace(name)).ToList();
        }

        private static string RemoveSpecialCharacters(this string source)
        {
            const string regExp = @"[^0-9A-Za-z ]";
            return Regex.Replace(source, regExp, "");
        }

        public static string LongestCommonSubsequence(this string source, string toCompare)
        {
            // Dynamic programing to find LCS
            var dp = new int[source.Length + 1, toCompare.Length + 1];
            int i, j;
            for (i = 1; i <= source.Length; i++)
            {
                for (j = 1; j <= toCompare.Length; j++)
                {
                    if (source[i - 1] == toCompare[j - 1])
                    {
                        dp[i, j] = dp[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                    }
                }
            }

            // Find the longest string
            i = source.Length;
            j = toCompare.Length;
            var idx = 0;
            var result = new char[dp[i, j]];
            while (dp[i, j] > 0)
            {
                // bigger than top and left
                if (dp[i, j] > dp[i - 1, j] && dp[i, j] > dp[i, j - 1])
                {
                    result[idx++] = source[i - 1];
                    i--;
                    j--;
                }
                else if (dp[i, j] == dp[i - 1, j])
                {
                    i--;
                }
                else if (dp[i, j] == dp[i, j - 1])
                {
                    j--;
                }
            }

            // Reverse result
            Array.Reverse(result);

            return new string(result);
        }
    }
}
