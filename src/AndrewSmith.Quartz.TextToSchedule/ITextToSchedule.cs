using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndrewSmith.Quartz.TextToSchedule
{
    /// <summary>
    /// Responsible for turning plain text into a schedule that can be registered with a Quartz scheduler.
    /// </summary>
    public interface ITextToSchedule
    {
        /// <summary>
        /// Determines whether the specified text is valid.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        ///   <c>true</c> if the specified text is valid; otherwise, <c>false</c>.
        /// </returns>
        bool IsValid(string text);

        /// <summary>
        /// Parses the specified text into a schedule object.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Returns null if the text is invalid.</returns>
        TextToScheduleResults Parse(string text);

        /// <summary>
        /// Parses the specified text into a schedule object with the given time zone.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <returns></returns>
        TextToScheduleResults Parse(string text, TimeZoneInfo timeZone);
    }
}
