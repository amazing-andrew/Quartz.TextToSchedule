using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quartz;
using Quartz.Impl.Calendar;
using Quartz.Spi;
using AndrewSmith.Quartz.TextToSchedule.Calendars;

namespace AndrewSmith.Quartz.TextToSchedule.Test
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



        public static void AssertHasTimeIntervalOf(ITrigger trigger, TimeSpan span)
        {
            ISimpleTrigger simpleTrigger = (ISimpleTrigger)trigger;
            Assert.AreEqual(span, simpleTrigger.RepeatInterval);
        }

        public static void AssertHasCalendarOfType<T>(RegisterGroup group) where T : ICalendar
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
        }

        public static void AssertWeeklyCalendarHasDayIncluded(RegisterGroup group, DayOfWeek includedDay)
        {
            var weeklyCalendar = FindCalendarOfType<LocalWeeklyCalendar>(group);
            Assert.IsTrue(!weeklyCalendar.IsDayExcluded(includedDay),
                "day of week of {0} was expected to be included in the calendar but wasn't.",
                includedDay);
        }
        public static void AssertWeeklyCalendarHasDayExcluded(RegisterGroup group, DayOfWeek includedDay)
        {
            var weeklyCalendar = FindCalendarOfType<LocalWeeklyCalendar>(group);
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

            var dailyCal = FindCalendarOfType<LocalDailyCalendar>(group);
            Assert.IsTrue(dailyCal.IsTimeIncluded(date), "{0} as expected to be included in the daily calendar but wasn't", date);
        }

        public static void AssertDailyCalendarIsTimeExcluded(RegisterGroup group, int hour, int minute, int sec)
        {
            DateTimeOffset date = DateTimeOffset.Now;
            date = new DateTimeOffset(date.Year, date.Month, date.Day,
                hour, minute, sec, date.Offset);

            date = date.ToUniversalTime();

            var dailyCal = FindCalendarOfType<LocalDailyCalendar>(group);
            Assert.IsFalse(dailyCal.IsTimeIncluded(date), "{0} as expected to be excluded in the daily calendar but wasn't", date);
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
