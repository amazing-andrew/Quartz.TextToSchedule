using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndrewSmith.Quartz.TextToSchedule
{
    /// <summary>
    /// Describes a time value in seconds, minutes, or hours
    /// </summary>
    public enum TimeValue
    {
        Seconds,
        Minutes,
        Hours
    }

    /// <summary>
    /// Describes an ordinal value
    /// </summary>
    public enum Ordinal
    {
        First,
        Second,
        Third,
        Fourth,
        Fifth,
        Last
    }
}
