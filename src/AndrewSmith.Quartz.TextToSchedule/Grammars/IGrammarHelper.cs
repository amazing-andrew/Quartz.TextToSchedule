using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndrewSmith.Quartz.TextToSchedule.Grammars
{
    /// <summary>
    /// Used to help pull values and understand the results of a grammar.
    /// </summary>
    public interface IGrammarHelper
    {
        /// <summary>
        /// Normalizes the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        string Normalize(string text);

        /// <summary>
        /// Gets the time value from string.
        /// </summary>
        /// <param name="timeValueString">The time value string.</param>
        /// <returns></returns>
        TimeValue GetTimeValueFromString(string timeValueString);

        /// <summary>
        /// Gets the amount value from string.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        int GetAmountValueFromString(string amount);

        /// <summary>
        /// Gets the date time from date spec.
        /// </summary>
        /// <param name="datespec">The datespec.</param>
        /// <returns></returns>
        DateTime? GetDateTimeFromDateSpec(string datespec);

        /// <summary>
        /// Gets the time from time string.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        DateTime? GetTimeFromTimeString(string time);

        /// <summary>
        /// Gets the date time from date spec and time.
        /// </summary>
        /// <param name="datespec">The datespec.</param>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        DateTime? GetDateTimeFromDateSpecAndTime(string datespec, string time);

        //day of week

        /// <summary>
        /// Gets the day of week from string.
        /// </summary>
        /// <param name="dayOfWeekString">The day of week string.</param>
        /// <returns></returns>
        DayOfWeek GetDayOfWeekFromString(string dayOfWeekString);

        /// <summary>
        /// Gets the day of week values.
        /// </summary>
        /// <param name="dayOfWeekSpecs">The day of week specs.</param>
        /// <returns></returns>
        List<DayOfWeek> GetDayOfWeekValues(string[] dayOfWeekSpecs);

        //month

        /// <summary>
        /// Gets an individual month value.
        /// </summary>
        /// <param name="monthSpecs">The month specs.</param>
        /// <returns></returns>
        int GetMonthValue(string month);

        /// <summary>
        /// Gets the month values.
        /// </summary>
        /// <param name="monthSpecs">The month specs.</param>
        /// <returns></returns>
        List<int> GetMonthValues(string[] monthSpecs);


        /// <summary>
        /// Gets an individual year value.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        int GetYearValue(string year);

        //ordinal

        /// <summary>
        /// Gets the ordinal values.
        /// </summary>
        /// <param name="ordinals">The ordinals.</param>
        /// <returns></returns>
        List<Ordinal> GetOrdinalValues(string[] ordinals);

        /// <summary>
        /// Gets an individual ordinal value.
        /// </summary>
        /// <param name="ordinal">The ordinal.</param>
        /// <returns></returns>
        Ordinal GetOrdinalValue(string ordinal);
    }
}
