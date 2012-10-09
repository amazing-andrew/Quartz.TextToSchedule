using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AndrewSmith.Quartz.TextToSchedule
{
    /// <summary>
    /// Helper class to help with common regular expression tasks.
    /// </summary>
    internal class RegexHelper
    {
        #region Matching

        /// <summary>
        /// Gets the regular expression captured groups that have names.
        /// </summary>
        /// <param name="regex">The regex.</param>
        /// <param name="m">The regular expression match.</param>
        /// <returns></returns>
        public static NameValueCollection GetNamedMatches(Regex regex, Match m)
        {
            NameValueCollection dic = new NameValueCollection();

            foreach (string groupName in regex.GetGroupNames())
            {
                if (Regex.IsMatch(groupName, @"^\d"))
                    continue;

                var groupCapture = m.Groups[groupName];
                if (groupCapture.Success)
                {
                    foreach (Capture capture in groupCapture.Captures)
                    {
                        dic.Add(groupName, capture.Value);
                    }
                }
            }

            return dic;
        }

        /// <summary>
        /// Gets the regular expression captured groups that have names.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static NameValueCollection GetNamedMatches(string input, string expression)
        {
            Regex regex = new Regex(expression);

            Match m = Regex.Match(input, expression);

            if (m.Success)
            {
                return GetNamedMatches(regex, m);
            }

            return null;
        }
        /// <summary>
        /// Wraps the pattern with "^" and "$" to match only the exact string contents.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns>
        ///   <c>true</c> if the specified input is a full match; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFullMatch(string input, string pattern)
        {
            if (!pattern.StartsWith("^"))
                pattern = "^" + pattern;
            if (!pattern.EndsWith("$"))
                pattern += "$";

            return Regex.IsMatch(input, pattern);
        }

        #endregion


        /// <summary>
        /// Wraps a pattern with a regular expression named capture.
        /// </summary>
        /// <param name="captureName">Name of the capture.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns></returns>
        public static string Builder_Capture(string captureName, string pattern)
        {
            return string.Format("(?<{0}>{1})", captureName, pattern);
        }


        /// <summary>
        /// Wraps a pattern to turn into into a list of the same value that is separated with a separator.
        /// </summary>
        /// <param name="regex">The regex.</param>
        /// <param name="LIST_SEPARATOR">The list separator regex.</param>
        /// <returns></returns>
        public static string Builder_ListOf(string regex, string LIST_SEPARATOR)
        {
            var list = regex + "(" + LIST_SEPARATOR + regex + ")*";
            return list;
        }

        /// <summary>
        /// Builds a named capture group that can be an match any one of the given alternates.
        /// </summary>
        /// <param name="captureName">Name of the capture group.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static string Builder_GroupOf(string captureName, string[] items)
        {
            return string.Format("(?<{0}>({1}))", captureName, string.Join("|", items));
        }


        /// <summary>
        /// group that can be an match any one of the given alternates.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static string Builder_GroupOf(string[] items)
        {
            return string.Format("({0})", string.Join("|", items));
        }

        /// <summary>
        /// Builds the range syntax between values.
        /// </summary>
        /// <param name="regex">The regex.</param>
        /// <param name="RANGE_SEPARATOR">The range separator syntax.</param>
        /// <returns></returns>
        public static string Builder_Range(string regex, string RANGE_SEPARATOR)
        {
            return Builder_Capture("RANGESTART", regex) +
                RANGE_SEPARATOR +
                Builder_Capture("RANGEEND", regex);

            //return "(" + regex + RANGE_SEPARATOR + regex + ")";
        }
    }
}
