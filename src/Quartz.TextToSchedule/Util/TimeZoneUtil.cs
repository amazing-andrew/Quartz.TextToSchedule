using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quartz.TextToSchedule.Util
{
    public class TimeZoneUtil
    {
        private static readonly bool isRunningOnMono = Type.GetType("Mono.Runtime") != null;

        /// <summary>
        /// TimeZoneInfo.ConvertTime is not supported under mono
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <param name="timeZoneInfo"></param>
        /// <returns></returns>
        public static DateTimeOffset ConvertTime(DateTimeOffset dateTimeOffset, TimeZoneInfo timeZoneInfo)
        {
            if (isRunningOnMono)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dateTimeOffset.UtcDateTime, timeZoneInfo);
            }

            return TimeZoneInfo.ConvertTime(dateTimeOffset, timeZoneInfo);
        }
    }
}
