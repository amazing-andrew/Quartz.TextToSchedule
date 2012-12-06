using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quartz.TextToSchedule
{
    public static class TextToScheduleExtensions
    {
        public static void ScheduleJob(this IScheduler scheduler, IJobDetail jobDetail, string scheduleText)
        {
            ScheduleJob(scheduler, jobDetail, scheduleText, TimeZoneInfo.Local);
        }
        public static void ScheduleJob(this IScheduler scheduler, IJobDetail jobDetail, string scheduleText, TimeZoneInfo timeZone)
        {
            TextToScheduleFactory factory = new TextToScheduleFactory();
            var english = factory.CreateEnglishParser();
            var results = english.Parse(scheduleText, timeZone);
            results.ScheduleWithJob(scheduler, jobDetail);
        }

        public static void ScheduleJob(this IScheduler scheduler, JobKey jobKey, string scheduleText)
        {
            ScheduleJob(scheduler, jobKey, scheduleText, TimeZoneInfo.Local);
        }
        public static void ScheduleJob(this IScheduler scheduler, JobKey jobKey, string scheduleText, TimeZoneInfo timeZone)
        {
            TextToScheduleFactory factory = new TextToScheduleFactory();
            var english = factory.CreateEnglishParser();
            var results = english.Parse(scheduleText, timeZone);
            results.ScheduleWithJobKey(scheduler, jobKey);
        }

        public static bool IsValidScheduleText(this IScheduler scheduler, string scheduleText)
        {
            TextToScheduleFactory factory = new TextToScheduleFactory();
            var english = factory.CreateEnglishParser();

            try
            {
                return english.IsValid(scheduleText);
            }
            catch
            {
                return false;
            }
        }
    }


}
