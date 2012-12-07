using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quartz;
using Quartz.Impl.Calendar;
using Quartz.Spi;
#if CUSTOM
using Quartz.TextToSchedule.Calendars;
#endif

namespace Quartz.TextToSchedule.Test
{
    public class TestHelper
    {
        public static void AssertHasCronExpression(TextToScheduleResults results , string cronExpression)
        {
            bool foundCronTrigger = false;

            foreach (var g in results.RegisterGroups)
            {
                var trigger = g.TriggerBuilder.Build();

                if (trigger is ICronTrigger)
                {
                    ICronTrigger cronTrigger = (ICronTrigger)trigger;
                    if (cronExpression == cronTrigger.CronExpressionString)
                    {
                        foundCronTrigger = true;
                        break;
                    }
                }
            }

            if (!foundCronTrigger)
                Assert.Fail(
                    string.Format("Could not find cron string of {0}, in the list of found cron expressions ({1}).",
                    cronExpression,
                    string.Join(", ", results.RegisterGroups.Select(x => x.TriggerBuilder.Build()).OfType<ICronTrigger>().Select(x => x.CronExpressionString).ToList())));
        }

        public static void AssertHasTimeIntervalOf(ITrigger trigger, int amount, IntervalUnit unit)
        {
            ICalendarIntervalTrigger calTrigger = (ICalendarIntervalTrigger)trigger;
            Assert.AreEqual(amount, calTrigger.RepeatInterval, "Repeat interval expected was {0}, but actual is {1}", amount, calTrigger.RepeatInterval);
            Assert.AreEqual(unit, calTrigger.RepeatIntervalUnit, "Repeat Interval Unit expected was {0}, but actual is {1}", unit, calTrigger.RepeatIntervalUnit);
        }

        public static void AssertWeeklyCalendarHasDayIncluded(RegisterGroup group, DayOfWeek includedDay)
        {

#if CUSTOM
            var weeklyCalendar = FindCalendarOfType<LocalWeeklyCalendar>(group);
#else
            var weeklyCalendar = FindCalendarOfType<WeeklyCalendar>(group);
#endif

            Assert.IsTrue(!weeklyCalendar.IsDayExcluded(includedDay),
                "day of week of {0} was expected to be included in the calendar but wasn't.",
                includedDay);
        }
        public static void AssertWeeklyCalendarHasDayExcluded(RegisterGroup group, DayOfWeek includedDay)
        {

#if CUSTOM
            var weeklyCalendar = FindCalendarOfType<LocalWeeklyCalendar>(group);
#else
            var weeklyCalendar = FindCalendarOfType<WeeklyCalendar>(group);
#endif
            Assert.IsTrue(weeklyCalendar.IsDayExcluded(includedDay),
                "day of week of {0} was expected to be excluded in the calendar but wasn't.",
                includedDay);
        }

        public static void AssertDailyCalendarIsTimeIncluded(RegisterGroup group, int hour, int minute, int sec)
        {
            DateTimeOffset date = DateTimeOffset.Now;
            date = new DateTimeOffset(date.Year, date.Month, date.Day,
                hour, minute, sec, date.Offset);

            date = date.ToUniversalTime();

#if CUSTOM
            var dailyCal = FindCalendarOfType<LocalDailyCalendar>(group);
#else
            var dailyCal = FindCalendarOfType<DailyCalendar>(group);
#endif
            Assert.IsTrue(dailyCal.IsTimeIncluded(date), "{0} as expected to be included in the daily calendar but wasn't", date);
        }
        public static void AssertDailyCalendarIsTimeExcluded(RegisterGroup group, int hour, int minute, int sec)
        {
            DateTimeOffset date = DateTimeOffset.Now;
            date = new DateTimeOffset(date.Year, date.Month, date.Day,
                hour, minute, sec, date.Offset);

            date = date.ToUniversalTime();

#if CUSTOM
            var dailyCal = FindCalendarOfType<LocalDailyCalendar>(group);
#else
            var dailyCal = FindCalendarOfType<DailyCalendar>(group);
#endif
            Assert.IsFalse(dailyCal.IsTimeIncluded(date), "{0} as expected to be excluded in the daily calendar but wasn't", date);
        }

        public static void AssertHasStartTimeOfDayOf(ITrigger trigger, int hour, int minute, int second)
        {
            TimeZoneInfo tz = TimeZoneInfo.Local;
            DateTimeOffset now = DateTimeOffset.Now;
            DateTimeOffset date = new DateTimeOffset(now.Year, now.Month, now.Day, hour, minute, second, TimeSpan.Zero);
            date = new DateTimeOffset(date.DateTime, tz.GetUtcOffset(date.DateTime));

            DateTimeOffset triggerDate = Quartz.TextToSchedule.Util.TimeZoneUtil.ConvertTime(trigger.StartTimeUtc, tz);

            Assert.AreEqual(triggerDate.Hour, date.Hour);
            Assert.AreEqual(triggerDate.Minute, date.Minute);
            Assert.AreEqual(triggerDate.Second, date.Second);
        }

        public static void AssertHasStartDateOf(ITrigger trigger, int year, int month, int day, int hour, int minute, int second)
        {
            TimeZoneInfo tz = TimeZoneInfo.Local;
            DateTimeOffset date = new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.Zero);
            date = new DateTimeOffset(date.DateTime, tz.GetUtcOffset(date.DateTime));

            Assert.AreEqual(trigger.StartTimeUtc, date);
        }


        private static T FindCalendarOfType<T>(RegisterGroup group) where T : ICalendar
        {
            bool found = false;
            ICalendar cal = group.Calendar;

            while (cal != null)
            {
                if (cal is T)
                {
                    found = true;
                    break;
                }

                cal = cal.CalendarBase;
            }

            if (!found)
                Assert.Fail("could not find a calendar or base calendar of type {0}", typeof(T));

            return (T)cal;
        }
    }
}
