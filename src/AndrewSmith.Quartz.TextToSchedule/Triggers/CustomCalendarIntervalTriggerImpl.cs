#if CUSTOM

using Quartz;
using Quartz.Impl.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewSmith.Quartz.TextToSchedule.Triggers
{
    [Serializable]
    public class CustomCalendarIntervalTriggerImpl : CalendarIntervalTriggerImpl
    {
        private static readonly int YearToGiveupSchedulingAt = DateTime.Now.AddYears(100).Year;
        private bool complete = false;

        /// <summary>
        /// Returns the next time at which the <see cref="ICalendarIntervalTrigger" /> will fire,
        /// after the given time. If the trigger will not fire after the given time,
        /// <see langword="null" /> will be returned.
        /// </summary>
        public override DateTimeOffset? GetFireTimeAfter(DateTimeOffset? afterTime)
        {
            return GetFireTimeAfter(afterTime, false);
        }

        public new DateTimeOffset? GetFireTimeAfter(DateTimeOffset? afterTime, bool ignoreEndTime)
        {
            if (complete)
            {
                return null;
            }

            // increment afterTme by a second, so that we are 
            // comparing against a time after it!
            if (afterTime == null)
            {
                afterTime = SystemTime.UtcNow().AddSeconds(1);
            }
            else
            {
                afterTime = afterTime.Value.AddSeconds(1);
            }

            DateTimeOffset startMillis = StartTimeUtc;
            DateTimeOffset afterMillis = afterTime.Value;
            DateTimeOffset endMillis = (EndTimeUtc == null) ? DateTimeOffset.MaxValue : EndTimeUtc.Value;

            if (!ignoreEndTime && (endMillis <= afterMillis))
            {
                return null;
            }

            if (afterMillis < startMillis)
            {
                return startMillis;
            }

            long secondsAfterStart = (long)(afterMillis - startMillis).TotalSeconds;

            DateTimeOffset? time = null;
            long repeatLong = RepeatInterval;

            DateTimeOffset sTime = StartTimeUtc;
            if (this.TimeZone != null)
            {
                sTime = TimeZoneInfo.ConvertTime(sTime, this.TimeZone);
            }

            if (RepeatIntervalUnit == IntervalUnit.Second)
            {
                long jumpCount = secondsAfterStart / repeatLong;
                if (secondsAfterStart % repeatLong != 0)
                {
                    jumpCount++;
                }
                time = sTime.AddSeconds(RepeatInterval * (int)jumpCount);


                //*********************************************************************
                // CUSTOM CODE: Adjust the time if we need to for Daylight Savings Time
                //*********************************************************************
                if (this.PreserveHourOfDayAcrossDaylightSavings && time.Value.ToLocalTime().Offset != sTime.Offset)
                {
                    var local = time.Value.ToLocalTime().Offset;
                    var start = sTime.Offset;

                    if (local < start)
                        time = time.Value.Add(start - local);
                    else
                        time = time.Value.Add(local - start);
                }
            }
            else if (RepeatIntervalUnit == IntervalUnit.Minute)
            {
                long jumpCount = secondsAfterStart / (repeatLong * 60L);
                if (secondsAfterStart % (repeatLong * 60L) != 0)
                {
                    jumpCount++;
                }
                time = sTime.AddMinutes(RepeatInterval * (int)jumpCount);

                //*********************************************************************
                // CUSTOM CODE: Adjust the time if we need to for Daylight Savings Time
                //*********************************************************************
                if (this.PreserveHourOfDayAcrossDaylightSavings && time.Value.ToLocalTime().Offset != sTime.Offset)
                {
                    var local = time.Value.ToLocalTime().Offset;
                    var start = sTime.Offset;

                    if (local < start)
                        time = time.Value.Add(start - local);
                    else
                        time = time.Value.Add(local - start);
                }

            }
            else if (RepeatIntervalUnit == IntervalUnit.Hour)
            {
                long jumpCount = secondsAfterStart / (repeatLong * 60L * 60L);
                if (secondsAfterStart % (repeatLong * 60L * 60L) != 0)
                {
                    jumpCount++;
                }
                time = sTime.AddHours(RepeatInterval * (int)jumpCount);

                //*********************************************************************
                // CUSTOM CODE: Adjust the time if we need to for Daylight Savings Time
                //*********************************************************************
                if (this.PreserveHourOfDayAcrossDaylightSavings && time.Value.ToLocalTime().Offset != sTime.Offset)
                {
                    var local = time.Value.ToLocalTime().Offset;
                    var start = sTime.Offset;

                    if (local < start)
                        time = time.Value.Add(start - local);
                    else
                        time = time.Value.Add(local - start);
                }
            }
            else
            {
                // intervals a day or greater ...

                int initialHourOfDay = sTime.Hour;

                if (RepeatIntervalUnit == IntervalUnit.Day)
                {
                    // Because intervals greater than an hour have an non-fixed number 
                    // of seconds in them (due to daylight savings, variation number of 
                    // days in each month, leap year, etc. ) we can't jump forward an
                    // exact number of seconds to calculate the fire time as we can
                    // with the second, minute and hour intervals.   But, rather
                    // than slowly crawling our way there by iteratively adding the 
                    // increment to the start time until we reach the "after time",
                    // we can first make a big leap most of the way there...

                    long jumpCount = secondsAfterStart / (repeatLong * 24L * 60L * 60L);
                    // if we need to make a big jump, jump most of the way there, 
                    // but not all the way because in some cases we may over-shoot or under-shoot
                    if (jumpCount > 20)
                    {
                        if (jumpCount < 50)
                        {
                            jumpCount = (long)(jumpCount * 0.80);
                        }
                        else if (jumpCount < 500)
                        {
                            jumpCount = (long)(jumpCount * 0.90);
                        }
                        else
                        {
                            jumpCount = (long)(jumpCount * 0.95);
                        }
                        sTime = sTime.AddDays(RepeatInterval * jumpCount);
                    }

                    // now baby-step the rest of the way there...
                    while (sTime < afterTime && sTime.Year < YearToGiveupSchedulingAt)
                    {
                        sTime = sTime.AddDays(RepeatInterval);
                    }
                    while (DaylightSavingHourShiftOccuredAndAdvanceNeeded(ref sTime, initialHourOfDay) && sTime.Year < YearToGiveupSchedulingAt)
                    {
                        sTime = sTime.AddDays(RepeatInterval);
                    }
                    time = sTime;
                }
                else if (RepeatIntervalUnit == IntervalUnit.Week)
                {
                    // Because intervals greater than an hour have an non-fixed number 
                    // of seconds in them (due to daylight savings, variation number of 
                    // days in each month, leap year, etc. ) we can't jump forward an
                    // exact number of seconds to calculate the fire time as we can
                    // with the second, minute and hour intervals.   But, rather
                    // than slowly crawling our way there by iteratively adding the 
                    // increment to the start time until we reach the "after time",
                    // we can first make a big leap most of the way there...

                    long jumpCount = secondsAfterStart / (repeatLong * 7L * 24L * 60L * 60L);
                    // if we need to make a big jump, jump most of the way there, 
                    // but not all the way because in some cases we may over-shoot or under-shoot
                    if (jumpCount > 20)
                    {
                        if (jumpCount < 50)
                        {
                            jumpCount = (long)(jumpCount * 0.80);
                        }
                        else if (jumpCount < 500)
                        {
                            jumpCount = (long)(jumpCount * 0.90);
                        }
                        else
                        {
                            jumpCount = (long)(jumpCount * 0.95);
                        }
                        sTime = sTime.AddDays((int)(RepeatInterval * jumpCount * 7));
                    }

                    while (sTime < afterTime && sTime.Year < YearToGiveupSchedulingAt)
                    {
                        sTime = sTime.AddDays(RepeatInterval * 7);
                    }
                    while (DaylightSavingHourShiftOccuredAndAdvanceNeeded(ref sTime, initialHourOfDay) && sTime.Year < YearToGiveupSchedulingAt)
                    {
                        sTime = sTime.AddDays(RepeatInterval * 7);
                    }
                    time = sTime;
                }
                else if (RepeatIntervalUnit == IntervalUnit.Month)
                {
                    // because of the large variation in size of months, and 
                    // because months are already large blocks of time, we will
                    // just advance via brute-force iteration.
                    while (sTime < afterTime && sTime.Year < YearToGiveupSchedulingAt)
                    {
                        sTime = sTime.AddMonths(RepeatInterval);
                    }
                    while (DaylightSavingHourShiftOccuredAndAdvanceNeeded(ref sTime, initialHourOfDay)
                           && sTime.Year < YearToGiveupSchedulingAt)
                    {
                        sTime = sTime.AddMonths(RepeatInterval);
                    }
                    time = sTime;
                }
                else if (RepeatIntervalUnit == IntervalUnit.Year)
                {
                    while (sTime < afterTime && sTime.Year < YearToGiveupSchedulingAt)
                    {
                        sTime = sTime.AddYears(RepeatInterval);
                    }
                    while (DaylightSavingHourShiftOccuredAndAdvanceNeeded(ref sTime, initialHourOfDay) && sTime.Year < YearToGiveupSchedulingAt)
                    {
                        sTime = sTime.AddYears(RepeatInterval);
                    }
                    time = sTime;
                }
            } // case of interval of a day or greater
            if (!ignoreEndTime && endMillis <= time)
            {
                return null;
            }

            return time;
        }

        private bool DaylightSavingHourShiftOccuredAndAdvanceNeeded(ref DateTimeOffset newTime, int initialHourOfDay)
        {
            DateTimeOffset toCheck = TimeZoneInfo.ConvertTime(newTime, this.TimeZone);

            if (PreserveHourOfDayAcrossDaylightSavings && toCheck.Hour != initialHourOfDay)
            {
                newTime = new DateTimeOffset(newTime.Year, newTime.Month, newTime.Day, initialHourOfDay, newTime.Minute, newTime.Second, newTime.Millisecond, toCheck.Offset);
                if (newTime.Hour != initialHourOfDay)
                {
                    return true;
                }
            }
            return false;
        }

        //must be true, otherwise this get's saved as a regular calendar interval trigger when
        //saved to a database.
        public override bool HasAdditionalProperties
        {
            get
            {
                return true;
            }
        }
    }
}
#endif