using Quartz;
using Quartz.Impl.Calendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AndrewSmith.Quartz.TextToSchedule.Calendars
{
    /// <summary>
    /// A custom calendar impl. I built this because the WeeklyCalendar looked at the Days of the Week according to 
    /// UTC time, which could be on a different day of week than the local time.
    /// </summary>
     [Serializable]
    public class LocalWeeklyCalendar : WeeklyCalendar
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalWeeklyCalendar" /> class.
        /// </summary>
        public LocalWeeklyCalendar () { }
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalWeeklyCalendar" /> class.
        /// </summary>
        /// <param name="baseCalendar">The base calendar.</param>
        public LocalWeeklyCalendar (ICalendar baseCalendar) : base(baseCalendar) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalWeeklyCalendar" /> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        public LocalWeeklyCalendar(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Determine whether the given time (in milliseconds) is 'included' by the
        /// Calendar.
        /// <para>
        /// Note that this Calendar is only has full-day precision.
        /// </para>
        /// </summary>
        /// <param name="timeUtc"></param>
        /// <returns></returns>
        public override bool IsTimeIncluded(DateTimeOffset timeUtc)
        {
            DateTimeOffset localTime = timeUtc.ToLocalTime();
            return base.IsTimeIncluded(localTime);
        }

        /// <summary>
        /// Determine the next time (in milliseconds) that is 'included' by the
        /// Calendar after the given time. Return the original value if timeStamp is
        /// included. Return DateTime.MinValue if all days are excluded.
        /// <para>
        /// Note that this Calendar is only has full-day precision.
        /// </para>
        /// </summary>
        /// <param name="timeUtc"></param>
        /// <returns></returns>
        public override DateTimeOffset GetNextIncludedTimeUtc(DateTimeOffset timeUtc)
        {
            if (base.AreAllDaysExcluded())
            {
                return DateTime.MinValue;
            }

            // Call base calendar implementation first
            DateTimeOffset baseTime = base.GetNextIncludedTimeUtc(timeUtc);
            if ((baseTime != DateTimeOffset.MinValue) && (baseTime > timeUtc))
            {
                timeUtc = baseTime;
            }

            // Get timestamp for 00:00:00
            //DateTime d = timeUtc.Date;   --commented out for local time impl
            DateTime d = timeUtc.ToLocalTime().Date;

            if (!IsDayExcluded(d.DayOfWeek))
            {
                return timeUtc;
            } // return the original value

            while (IsDayExcluded(d.DayOfWeek))
            {
                d = d.AddDays(1);
            }

            return d;
        }
    }
}
