using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Quartz.TextToSchedule
{
    /// <summary>
    /// An <see cref="ITextToSchedule"/> that supports cron expressions.
    /// </summary>
    public class CronTextToSchedule : ITextToSchedule
    {
        private static readonly string[] Delimiters = new string[] { ",", ";" };

        /// <summary>
        /// Prepares the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        private string[] Prepare(string text)
        {
            if (text == null)
                return null;

            var all = text.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
            return all.Select(x => Regex.Replace(x.Trim().ToUpper(), "\\ +", " ")).ToArray();
        }

        /// <summary>
        /// Determines whether the specified text is valid.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        ///   <c>true</c> if the specified text is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid(string text)
        {
            var expressions = Prepare(text);

            foreach (var exp in expressions)
            {
                if (!CronExpression.IsValidExpression(exp))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Parses the specified text into a schedule object.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        /// Returns null if the text is invalid.
        /// </returns>
        public TextToScheduleResults Parse(string text)
        {
            return Parse(text, TimeZoneInfo.Local);
        }

        /// <summary>
        /// Parses the specified text into a schedule object with the given time zone.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <returns></returns>
        public TextToScheduleResults Parse(string text, TimeZoneInfo timeZone)
        {
            if (text == null)
                return null;

            var expressions = Prepare(text);
            TextToScheduleResults results = new TextToScheduleResults();

            foreach (var cron in expressions)
            {
                results.Add(
                    TriggerBuilder.Create()
                    .WithCronSchedule(cron, sb => sb.InTimeZone(timeZone)));
            }

            return results;
        }
    }
}
