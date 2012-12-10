using Quartz.TextToSchedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quartz.TextToSchedule
{
    public class MultiTextToSchedule : ITextToSchedule
    {
        public IList<ITextToSchedule> Parsers { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiTextToSchedule" /> class.
        /// </summary>
        public MultiTextToSchedule()
        {
            Parsers = new List<ITextToSchedule>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiTextToSchedule" /> class.
        /// </summary>
        /// <param name="parsers">The parsers.</param>
        public MultiTextToSchedule(params ITextToSchedule[] parsers)
        {
            Parsers = parsers.ToList();
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
            foreach (var item in Parsers)
            {
                if (item.IsValid(text))
                    return true;
            }

            return false;
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
            foreach (var item in Parsers)
            {
                if (item.IsValid(text))
                    return item.Parse(text);
            }

            return null;
        }

        /// <summary>
        /// Parses the specified text into a schedule object with the given time zone.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <returns></returns>
        public TextToScheduleResults Parse(string text, TimeZoneInfo timeZone)
        {
            foreach (var item in Parsers)
            {
                if (item.IsValid(text))
                    return item.Parse(text, timeZone);
            }

            return null;
        }
    }
}
