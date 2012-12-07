using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Quartz.TextToSchedule.Sample.German
{
    internal static class RegexHelper
    {
        #region Matching

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
        public static bool IsFullMatch(string input, string pattern)
        {
            if (!pattern.StartsWith("^"))
                pattern = "^" + pattern;
            if (!pattern.EndsWith("$"))
                pattern += "$";

            return Regex.IsMatch(input, pattern);
        }

        #endregion

        public static string Builder_Capture(string captureName, string pattern)
        {
            return string.Format("(?<{0}>{1})", captureName, pattern);
        }
        public static string Builder_ListOf(string regex, string LIST_SEPARATOR)
        {
            var list = regex + "(" + LIST_SEPARATOR + regex + ")*";
            return list;
        }

        public static string Builder_GroupOf(string captureName, string[] items)
        {
            return string.Format("(?<{0}>({1}))", captureName, string.Join("|", items));
        }
        public static string Builder_GroupOf(string[] items)
        {
            return string.Format("({0})", string.Join("|", items));
        }

        public static string Builder_Range(string regex, string RANGE_SEPARATOR)
        {
            return Builder_Capture("RANGESTART", regex) +
                RANGE_SEPARATOR +
                Builder_Capture("RANGEEND", regex);

            //return "(" + regex + RANGE_SEPARATOR + regex + ")";
        }
    }
}
