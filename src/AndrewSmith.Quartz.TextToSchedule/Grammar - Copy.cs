using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AndrewSmith.TextToSchedule
{
    public class Grammar
    {
        public static readonly string SPACE = @"\ ";
        public static readonly string LIST_SEPARATOR = @"(,|;|\/|\\|\ |\ and\ )";
        public static readonly string RANGE_SEPARATOR = "(" + SPACE + "through" + SPACE + "|" + SPACE + "thru" + SPACE + "|" + SPACE + "?\\-" + SPACE + "?)";
        public static readonly string AMOUNT = @"(?<AMOUNT>\d+)";

        #region Patterns - TIMEVALUE

        //TIMEVALUE
        //regular time value is (seconds|minutes|hours)
        public static readonly string TIMEVALUE_SECOND = "(s|sec|secs|second|seconds)";
        public static readonly string TIMEVALUE_MINUTE = "(m|min|mins|minute|minutes)";
        public static readonly string TIMEVALUE_HOUR = "(h|hr|hrs|hour|hours)";

        public static readonly string TIMEVALUE_DAY = "(day|days)";
        public static readonly string TIMEVALUE_WEEK = "(week|weeks)";
        public static readonly string TIMEVALUE_MONTH = "(month|months|mth)";
        public static readonly string TIMEVALUE_YEAR = "(yr|year|years)";

        public static readonly string TIMEVALUE = RegexHelper.Builder_GroupOf("TIMEVALUE", new string[] { TIMEVALUE_SECOND, TIMEVALUE_MINUTE, TIMEVALUE_HOUR });
        public static readonly string TIMEVALUE_EXTENDED = RegexHelper.Builder_GroupOf("TIMEVALUE", new string[] { TIMEVALUE_SECOND, TIMEVALUE_MINUTE, TIMEVALUE_HOUR, TIMEVALUE_DAY, TIMEVALUE_WEEK, TIMEVALUE_YEAR });

        #endregion

        #region Patterns - DAYOF WEEK

        //DAYOFWEEK
        public static readonly string MONDAY = @"(mon|monday)";
        public static readonly string TUESDAY = @"(tu|tue|tuesday)";
        public static readonly string WEDNESDAY = @"(wed|wednesday)";
        public static readonly string THURSDAY = @"(th|thu|thur|thursday)";
        public static readonly string FRIDAY = @"(fri|friday)";
        public static readonly string SATURDAY = @"(sat|saturday)";
        public static readonly string SUNDAY = @"(sun|sunday)";

        public static readonly string DAYOFWEEK_ONE = RegexHelper.Builder_GroupOf(new string[] { MONDAY, TUESDAY, WEDNESDAY, THURSDAY, FRIDAY, SATURDAY, SUNDAY });
        public static readonly string DAYOFWEEK_RANGE = RegexHelper.Builder_Range(DAYOFWEEK_ONE, RANGE_SEPARATOR);
        public static readonly string DAYOFWEEK = RegexHelper.Builder_GroupOf("DAYOFWEEK", new string[] { DAYOFWEEK_ONE, DAYOFWEEK_RANGE });

        public static readonly string DAYOFWEEK_SPEC = RegexHelper.Builder_ListOf(DAYOFWEEK, LIST_SEPARATOR);

        #endregion

        #region Patterns - MONTHS

        //MONTHS
        public static readonly string JANUARY = "(jan|january)";
        public static readonly string FEBRUARY = "(feb|february)";
        public static readonly string MARCH = "(mar|march)";
        public static readonly string APRIL = "(apr|april)";
        public static readonly string MAY = "(may|may)";
        public static readonly string JUNE = "(jun|june)";
        public static readonly string JULY = "(jul|july)";
        public static readonly string AUGUST = "(aug|august)";
        public static readonly string SEPTEMBER = "(sep|september)";
        public static readonly string OCTOBER = "(oct|october)";
        public static readonly string NOVEMBER = "(nov|november)";
        public static readonly string DECEMBER = "(dec|december)";

        public static readonly string MONTH_ONE = RegexHelper.Builder_GroupOf(new string[] { JANUARY, FEBRUARY, MARCH, APRIL, MAY, JUNE, JULY, AUGUST, SEPTEMBER, OCTOBER, NOVEMBER, DECEMBER });
        public static readonly string MONTH_RANGE = RegexHelper.Builder_Range(MONTH_ONE, RANGE_SEPARATOR);
        public static readonly string MONTH = RegexHelper.Builder_GroupOf("MONTH", new string[] { MONTH_ONE, MONTH_RANGE });

        public static readonly string MONTH_SPEC = RegexHelper.Builder_ListOf(MONTH, LIST_SEPARATOR);

        #endregion

        #region Patterns - ORDINAL

        //ORDINAL
        public static readonly string ORDINAL_FIRST = "(first|1st)";
        public static readonly string ORDINAL_SECOND = "(second|2nd)";
        public static readonly string ORDINAL_THIRD = "(third|3rd)";
        public static readonly string ORDINAL_FOURTH = "(fourth|4th)";
        public static readonly string ORDINAL_LAST = "(last)";

        public static readonly string ORDINAL = RegexHelper.Builder_GroupOf("ORDINAL", new string[] { ORDINAL_FIRST, ORDINAL_SECOND, ORDINAL_THIRD, ORDINAL_FOURTH, ORDINAL_LAST });

        #endregion

        #region Patterns - TIME

        //TIME
        public static readonly string TIME_HOUR = @"(?<HOUR>\d|[0-1]\d|2[0-3])";
        public static readonly string TIME_MINUTE = @"(?<MINUTE>[0-5]\d)";
        public static readonly string TIME_SECOND = @"(?<SECOND>[0-5]\d)";
        public static readonly string TIME_AM = @"(am|a\.m\.|a\.m|am\.)";
        public static readonly string TIME_PM = @"(pm|p\.m\.|p\.m|pm\.)";
        public static readonly string TIME_MERIDIEM = RegexHelper.Builder_GroupOf("MERIDIEM", new string[] { TIME_AM, TIME_PM });

        public static readonly string TIME = @"(?<TIME>" + TIME_HOUR + "((:|" + SPACE + ")" + TIME_MINUTE + ")?((:|" + SPACE + ")" + TIME_SECOND + ")?" + SPACE + "?" + TIME_MERIDIEM + "?)";

        public static readonly string TIME_ONCE = "((at" + SPACE + ")?" + TIME + ")";
        public static readonly string TIME_RANGE = "(from" + SPACE + RegexHelper.Builder_Capture("FROMTIME", TIME) + SPACE + "to" + SPACE + RegexHelper.Builder_Capture("TOTIME", TIME) + ")";
        public static readonly string TIMESPEC = RegexHelper.Builder_GroupOf("TIMESPEC", new string[] { TIME_ONCE, TIME_RANGE });

        #endregion

        #region Patterns - DATE

        public static readonly string DATE_DAY = @"(?<DAY>[0-9]|1[1-9]|2[1-9]|3[1-3])(st|nd|rd|th)?";
        public static readonly string DATE = RegexHelper.Builder_GroupOf("MONTH", new string[] { MONTH_ONE }) + SPACE + "?,?" + SPACE + DATE_DAY;
        public static readonly string DATESPEC = "(?<DATESPEC>" + DATE + ")";

        #endregion

        // ---------------------
        // EXPRESSIONS
        // ---------------------

        //////  SYNTAX: every [n] (seconds|minutes|hours) [timespec]
        public static readonly string expr1 = "(every" + "(" + SPACE + AMOUNT + ")?" + SPACE + TIMEVALUE + "(" + SPACE + TIMESPEC + ")?" + ")";
        public static readonly string expr1_2 = "(every( {AMOUNT})? {TIMEVALUE}( {TIMESPEC})?)";

        //////  SYNTAX: ("every"|ordinal) (day of week) ["of" (monthspec)] (time)
        public static readonly string expr2 = "((every|" + ORDINAL + ")" + "(" + SPACE + "(day|" + DAYOFWEEK_SPEC + ")" + ")" + "(" + SPACE + "of" + SPACE + "(month|" + MONTH_SPEC + ")" + ")?" + "(" + SPACE + TIMESPEC + ")?" + ")";

        //////  SYNTAX: every [n] (second|minute|hour) "on" (day of week) of monthspec (time)
        public static readonly string expr3 = "(every" + "(" + SPACE + AMOUNT + ")?" + SPACE + TIMEVALUE + "(" + SPACE + "on" + SPACE + DAYOFWEEK_SPEC + ")?" + "(" + SPACE + "of" + SPACE + MONTH_SPEC + ")?" + "(" + SPACE + TIMESPEC + ")?" + ")";

        //[every [n] (second|minute|hour)] on (datespec) (time)
        public static readonly string expr4 = "((every" + "(" + SPACE + AMOUNT + ")?" + SPACE + TIMEVALUE + SPACE + ")?" +
            "on" + SPACE + DATESPEC + "(" + SPACE + TIMESPEC + ")?" + ")";

        //every [n] week(s) on (day of week) (time)
        //this only really supports specifiy a singluar day of week
        public static readonly string expr5 = "(every" + "(" + SPACE + AMOUNT + ")?" + SPACE + "weeks?" + "(" + SPACE + "on" + SPACE + RegexHelper.Builder_Capture("DAYOFWEEK", DAYOFWEEK_ONE) + ")?" + "(" + SPACE + TIMESPEC + ")?" + ")";

        //Expression Ideas
        //Implemented Ideas
        //1) every [n] (seconds|minutes|hours) [timespec]
        //2) ("every"|ordinal) (day of week) ["of" (monthspec)] (time)
        //3) every [n] (second|minute|hour) "on" (day of week) of monthspec (time)
        //4) every (datespec) at (time)

        //Other Ideas
        //every [n] (second|minute|hour|day) "of" monthspec (time)
        //every [n] (second|minute|hour) "on" (day of week) (time)
        //every [n] (second|minute|hour) "on" datespec (time)
        //every [n] weeks of (monthspec) (time)
        //every [n] month(s) from (month_once|datespec) (time)             every 3 months from february (3rd) at 4pm, every 3 months at 5pm
        //every [n] month(s) from (DATE_DAY) (time)                        every 3 months on the 1st at 4pm
        //every [n] year(s) on (datespec) (time)                           every 2 years on january 3rd at 4:35am

        // support, last day offsets, like second to last day, by using "L-[n]", where n is offset.
        // support nearest weekday "W".

        
        static Grammar()
        {
            //expressions 
            expr1_2 = NamedFormatExtension.NamedFormat(expr1_2, typeof(Grammar))
                .Replace(" ", SPACE);
        }
    }
}
