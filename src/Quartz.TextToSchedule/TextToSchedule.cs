#if CUSTOM
using Quartz.TextToSchedule.Calendars;
using Quartz.TextToSchedule.Triggers;
#endif
using Quartz.TextToSchedule.Grammars;
using Quartz;
using Quartz.Impl.Calendar;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz.TextToSchedule.Util;

namespace Quartz.TextToSchedule
{
    /// <summary>
    /// Default Impl of an <see cref="ITextToSchedule" />
    /// </summary>
    public class TextToSchedule : ITextToSchedule
    {
        /// <summary>
        /// Gets the grammar associated with this instance.
        /// </summary>
        /// <value>
        /// The grammar.
        /// </value>
        public IGrammar Grammar { get; private set; }

        /// <summary>
        /// Gets the grammar helper associated with this instance.
        /// </summary>
        /// <value>
        /// The grammar helper.
        /// </value>
        public IGrammarHelper GrammarHelper { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextToSchedule" /> class.
        /// </summary>
        /// <param name="grammar">The grammar.</param>
        /// <param name="helper">The helper.</param>
        /// <exception cref="System.ArgumentNullException">grammar</exception>
        public TextToSchedule(IGrammar grammar, IGrammarHelper helper)
        {
            if (grammar == null)
                throw new ArgumentNullException("grammar");

            if (helper == null)
                throw new ArgumentNullException("helper");

            Grammar = grammar;
            GrammarHelper = helper;
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
            try
            {
                var results = Parse(text);
                return (results.RegisterGroups.Count > 0);
            }
            catch
            {
                return false;
            }
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
            return Parse(text, TimeZoneInfo.Local);
        }

        /// <summary>
        /// Parses the specified text into a schedule object with the given time zone.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <returns></returns>
        public TextToScheduleResults Parse(string text, TimeZoneInfo timeZone)
        {
            text = GrammarHelper.Normalize(text);

            TextToScheduleResults results = new TextToScheduleResults();
            bool matched = false;

            if (ExecuteMatch(Grammar.Expression1, text, timeZone, results, Expression1Handler))
            {
                matched = true;
            }
            else if (ExecuteMatch(Grammar.Expression2, text, timeZone, results, Expression2Handler))
            {
                matched = true;
            }
            else if (ExecuteMatch(Grammar.Expression3, text, timeZone, results, Expression3Handler))
            {
                matched = true;
            }
            else if (ExecuteMatch(Grammar.Expression4, text, timeZone, results, Expression4Handler))
            {
                matched = true;
            }


            if (matched)
            {
                return results;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Handles with a given text matches the Expression1 field.
        /// </summary>
        /// <param name="nameValueCollection">A collection of values from the named capture groups.</param>
        /// <param name="results">The results.</param>
        private void Expression1Handler(NameValueCollection nameValueCollection, TimeZoneInfo timeZone, TextToScheduleResults results)
        {
            var amountString = nameValueCollection["AMOUNT"];
            var intervalUnitString = nameValueCollection["INTERVALUNIT"];
            var startDateString = nameValueCollection["DATESPEC"];

            var time = nameValueCollection["TIME"];
            var fromTime = nameValueCollection["FROMTIME"];
            var toTime = nameValueCollection["TOTIME"];

            var dayOfWeekSpecs = nameValueCollection.GetValues("DAYOFWEEK");
            var monthSpecs = nameValueCollection.GetValues("MONTH");

            DateTime? triggerStartTime = null;

            ICalendar calendar = null;

            //DAY OF WEEK SPECS
            if (dayOfWeekSpecs != null)
            {
                calendar = BuildCalendarOnDayOfWeek(calendar, dayOfWeekSpecs, timeZone);
            }

            //MONTH SPECS
            if (monthSpecs != null)
            {
                calendar = BuildCalendarOnMonths(calendar, monthSpecs, timeZone);
            }


            //TIME (single or range)

            //check for ranged time
            if (fromTime != null && toTime != null)
            {
                calendar = BuildCalendarOnTimeRange(calendar, fromTime, toTime, timeZone);

                //set the start date as the from time
                DateTime? fromTimeStartDate = GrammarHelper.GetTimeFromTimeString(fromTime);
                triggerStartTime = fromTimeStartDate;
            }
            //is regular time, process as single time provided
            else if (time != null)
            {
                DateTime? timeStartDate = GrammarHelper.GetTimeFromTimeString(time);
                triggerStartTime = timeStartDate;
            }

            //BUILD TRIGGER
            TriggerBuilder triggerBuilder = TriggerBuilder.Create();

            //set schedule
            triggerBuilder.WithSchedule(CreateScheduleWithAmountAndIntervalUnit(amountString, intervalUnitString, timeZone));


            //start on from time
            if (triggerStartTime != null)
                triggerBuilder.StartAt(new DateTimeOffset(triggerStartTime.Value, timeZone.GetUtcOffset(triggerStartTime.Value)));

            results.Add(triggerBuilder, calendar);
        }

        /// <summary>
        /// Handles with a given text matches the Expression2 field.
        /// </summary>
        /// <param name="nameValueCollection">A collection of values from the named capture groups.</param>
        /// <param name="results">The results.</param>
        private void Expression2Handler(NameValueCollection nameValueCollection, TimeZoneInfo timeZone, TextToScheduleResults results)
        {
            var time = nameValueCollection["TIME"];
            var fromTime = nameValueCollection["FROMTIME"];
            var toTime = nameValueCollection["TOTIME"];

            var dayOfWeekSpecs = nameValueCollection.GetValues("DAYOFWEEK");
            var monthSpecs = nameValueCollection.GetValues("MONTH");

            var ordinals = nameValueCollection.GetValues("ORDINAL");

            DateTime date = DateTime.Today; //default date to today

            if (time != null)
            {
                date = GrammarHelper.GetTimeFromTimeString(time).Value;
            }

            //init cron values
            List<string> cronExpressions = new List<string>();

            string cron_sec = date.Second.ToString();
            string cron_min = date.Minute.ToString();
            string cron_hour = date.Hour.ToString();
            string cron_day = "?";
            string cron_month = "*";
            string cron_dayofWeek = "*";

            if (monthSpecs != null)
            {
                var months = GrammarHelper.GetMonthValues(monthSpecs);
                cron_month = string.Join(",", months.Select(mon => GetMonthCronValue(mon)));
            }

            if (dayOfWeekSpecs != null)
            {
                var dows = GrammarHelper.GetDayOfWeekValues(dayOfWeekSpecs);
                cron_dayofWeek = string.Join(",", dows.Select(x => GetDayOfWeekCronValue(x)));
            }

            if (ordinals != null)
            {
                if (dayOfWeekSpecs != null)
                {
                    //combine ordinals and dayOfWeeks
                    var combined =
                        from a in GrammarHelper.GetOrdinalValues(ordinals)
                        from b in GrammarHelper.GetDayOfWeekValues(dayOfWeekSpecs)
                        select new { Ordinal = a, DayOfWeek = b };

                    foreach (var item in combined)
                    {
                        cron_dayofWeek = GetDayOfWeekCronValue(item.DayOfWeek);
                        cron_dayofWeek += GetOrdinalCronValue(item.Ordinal);

                        string cronString = string.Format(string.Join(" ", new string[] { cron_sec, cron_min, cron_hour, cron_day, cron_month, cron_dayofWeek }));
                        cronExpressions.Add(cronString);
                    }
                }

                if (dayOfWeekSpecs == null) //day was specified, handle special case
                {
                    //handle special cases
                    cron_dayofWeek = "?";

                    foreach (var o in GrammarHelper.GetOrdinalValues(ordinals))
                    {
                        cron_day = GetOrdinalCronValue(o).Replace("#", "");
                        string cronString = string.Format(string.Join(" ", new string[] { cron_sec, cron_min, cron_hour, cron_day, cron_month, cron_dayofWeek }));
                        cronExpressions.Add(cronString);
                    }
                }
            }
            else //no ordinal was specified
            {
                string cronString = string.Format(string.Join(" ", new string[] { cron_sec, cron_min, cron_hour, cron_day, cron_month, cron_dayofWeek }));
                cronExpressions.Add(cronString);
            }

            foreach (var cron in cronExpressions)
            {
                var triggerBuilder = TriggerBuilder.Create();

                IScheduleBuilder schedule = CreateScheduleWithCron(cron, timeZone);
                triggerBuilder.WithSchedule(schedule);

                results.Add(triggerBuilder);
            }
        }

        /// <summary>
        /// Handles with a given text matches the Expression3 field.
        /// </summary>
        /// <param name="nameValueCollection">A collection of values from the named capture groups.</param>
        /// <param name="results">The results.</param>
        private void Expression3Handler(NameValueCollection nameValueCollection, TimeZoneInfo timeZone, TextToScheduleResults results)
        {
            var time = nameValueCollection["TIME"];

            var dateSpec = nameValueCollection["DATESPEC"];
            var monthString = nameValueCollection["MONTH"];
            var dayString = nameValueCollection["DAY"];
            var yearString = nameValueCollection["YEAR"];

            DateTime date = DateTime.Today; //default date to today

            if (time != null)
            {
                date = GrammarHelper.GetTimeFromTimeString(time).Value;
            }

            //init cron values
            List<string> cronExpressions = new List<string>();

            string cron_sec = date.Second.ToString();
            string cron_min = date.Minute.ToString();
            string cron_hour = date.Hour.ToString();
            string cron_day = "*";
            string cron_month = "*";
            string cron_dayofWeek = "?";
            string cron_year = null;

            cron_month = GetMonthCronValue(GrammarHelper.GetMonthValue(monthString));

            if (dayString != null)
                cron_day = GetDayCronValue(GrammarHelper.GetDayValue(dayString));
            else
                cron_day = GetDayCronValue(1);

            if (yearString != null)
                cron_year = GetYearCronValue(GrammarHelper.GetYearValue(yearString));


            //build cron string
            string cronString = null;

            if (cron_year != null)
                cronString = string.Format(string.Join(" ", new string[] { cron_sec, cron_min, cron_hour, cron_day, cron_month, cron_dayofWeek, cron_year }));
            else
                cronString = string.Format(string.Join(" ", new string[] { cron_sec, cron_min, cron_hour, cron_day, cron_month, cron_dayofWeek }));

            //add cron string
            cronExpressions.Add(cronString);

            foreach (var cron in cronExpressions)
            {
                var triggerBuilder = TriggerBuilder.Create();

                IScheduleBuilder schedule = CreateScheduleWithCron(cron, timeZone);
                triggerBuilder.WithSchedule(schedule);

                results.Add(triggerBuilder);
            }
        }

        /// <summary>
        /// Handles with a given text matches the Expression4 field.
        /// </summary>
        /// <param name="nameValueCollection">A collection of values from the named capture groups.</param>
        /// <param name="results">The results.</param>
        private void Expression4Handler(NameValueCollection nameValueCollection, TimeZoneInfo timeZone, TextToScheduleResults results)
        {
            // every [n] (days|weeks|months|years) (from [date]) (at [time])

            string amountString = nameValueCollection["AMOUNT"];
            string intervalString = nameValueCollection["INTERVALUNIT"];

            var dateSpec = nameValueCollection["DATESPEC"];
            var timeString = nameValueCollection["TIME"];

            DateTime? triggerStartTime = null;

            if (dateSpec != null || timeString != null)
                triggerStartTime = GrammarHelper.GetDateTimeFromDateSpecAndTime(dateSpec, timeString);

            TriggerBuilder triggerBuilder = TriggerBuilder.Create();

            triggerBuilder.WithSchedule(CreateScheduleWithAmountAndIntervalUnit(amountString, intervalString, timeZone));

            //start on from time
            if (triggerStartTime != null)
                triggerBuilder.StartAt(new DateTimeOffset(triggerStartTime.Value, timeZone.GetUtcOffset(triggerStartTime.Value)));

            results.Add(triggerBuilder, null);
        }


        #region Matching Helper Methods

        /// <summary>
        /// Method that matches an expression, and if it does will run the associated method
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="input">The input.</param>
        /// <param name="results">The results.</param>
        /// <param name="matchFunction">The match function to run if it matches.</param>
        /// <returns></returns>
        private static bool ExecuteMatch(string expression, string input, TimeZoneInfo timeZone, TextToScheduleResults results, Action<NameValueCollection, TimeZoneInfo, TextToScheduleResults> matchFunction)
        {
            expression = "^" + expression + "$";

            var dic = RegexHelper.GetNamedMatches(input, expression);

            if (dic != null)
            {
                matchFunction(dic, timeZone, results);
                return true;
            }

            return false;
        }

        #endregion

        #region Calendar & Helper Methods

        /// <summary>
        /// Builds a <see cref="LocalWeeklyCalendar"/> based on the given allowed days of weeks.
        /// </summary>
        /// <param name="dayofWeekSpecs">The day of week specs.</param>
        /// <returns></returns>
        private ICalendar BuildCalendarOnDayOfWeek(ICalendar baseCalendar, string[] dayofWeekSpecs, TimeZoneInfo timeZone)
        {
            //create calendar and exclude all days
#if CUSTOM
            LocalWeeklyCalendar calendar = null;

            if (baseCalendar != null)
                calendar = new LocalWeeklyCalendar(baseCalendar);
            else
                calendar = new LocalWeeklyCalendar();
#else
            WeeklyCalendar calendar = null;

            if (baseCalendar != null)
                calendar = new WeeklyCalendar(baseCalendar);
            else
                calendar = new WeeklyCalendar();
#endif

            calendar.DaysExcluded = new bool[7] { true, true, true, true, true, true, true };

            var dayOfWeeks = GrammarHelper.GetDayOfWeekValues(dayofWeekSpecs);

            foreach (var item in dayOfWeeks)
            {
                calendar.SetDayExcluded(item, false);
            }

            calendar.TimeZone = timeZone;
            return calendar;
        }

        /// <summary>
        /// Builds a <see cref="DailyCalendar"/> on the given allowed hours to run.
        /// </summary>
        /// <param name="fromTimeString">From time.</param>
        /// <param name="toTimeString">To time.</param>
        /// <returns></returns>
        private ICalendar BuildCalendarOnTimeRange(ICalendar baseCalendar, string fromTimeString, string toTimeString, TimeZoneInfo timeZone)
        {
            DateTime? dFromTime = GrammarHelper.GetTimeFromTimeString(fromTimeString);
            DateTime? dToTime = GrammarHelper.GetTimeFromTimeString(toTimeString);

            DateTime fromTime = dFromTime.Value;
            DateTime toTime = dToTime.Value;

            //adjust the utc month,day,year to match each other
            toTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, toTime.Hour, toTime.Minute, toTime.Second, toTime.Millisecond);

            bool shouldInvertTimeRange = false; //false = exclusive time range

            //if the toTime is lower than fromTime
            if (toTime < fromTime)
            {
                //switch the from and to times
                DateTime fromTemp = fromTime;
                fromTime = toTime.AddMilliseconds(1);
                toTime = fromTemp.AddMilliseconds(-1);
            }
            else
            {
                //check to see if they are the same
                //TODO: do something about this hacking the extra second
                if (fromTime.Equals(toTime))
                {
                    toTime = toTime.AddSeconds(1);
                }
                shouldInvertTimeRange = true; //turn this into an inclusive range
            }

#if CUSTOM
            LocalDailyCalendar calendar = null;
            if (baseCalendar != null)
                calendar = new LocalDailyCalendar(baseCalendar, fromTime, toTime);
            else
                calendar = new LocalDailyCalendar(fromTime, toTime);

            calendar.InvertTimeRange = shouldInvertTimeRange;
#else
            DailyCalendar calendar = null;
            if (baseCalendar != null)
                calendar = new DailyCalendar(baseCalendar, fromTime, toTime);
            else
                calendar = new DailyCalendar(fromTime, toTime);

            calendar.InvertTimeRange = shouldInvertTimeRange;
#endif

            calendar.TimeZone = timeZone;
            return calendar;
        }

        /// <summary>
        /// Builds a <see cref="ICalendar"/> based on the allowed months.
        /// </summary>
        /// <param name="monthSpecs">The month specs.</param>
        /// <returns></returns>
        private ICalendar BuildCalendarOnMonths(ICalendar baseCalendar, string[] monthSpecs, TimeZoneInfo timeZone)
        {
            int maxMonths = 12;
            var months = GrammarHelper.GetMonthValues(monthSpecs);

            //need exclusive so remove anything that we don't have
            List<int> toExclude = new List<int>();

            for (int i = 1; i < maxMonths + 1; i++)
            {
                toExclude.Add(i);
            }

            //remove any values that we already have
            foreach (var item in months)
            {
                toExclude.Remove(item);
            }

            string exclusiveCronExpression = string.Format("* * * * {0} ?", string.Join(",", toExclude));

            CronCalendar cronCal = null;

            if (baseCalendar != null)
                cronCal = new CronCalendar(baseCalendar, exclusiveCronExpression);
            else
                cronCal = new CronCalendar(exclusiveCronExpression);

            cronCal.TimeZone = timeZone;
            return cronCal;
        }

        #region Cron Value Strings

        /// <summary>
        /// Gets the day cron value.
        /// </summary>
        /// <param name="dayValue">The day value.</param>
        /// <returns></returns>
        private string GetDayCronValue(int dayValue)
        {
            return dayValue.ToString();
        }
        /// <summary>
        /// Gets the cron year string and converts 2 digit years into 4 digit years.
        /// </summary>
        /// <param name="yearValue">The year value.</param>
        /// <returns></returns>
        private string GetYearCronValue(int yearValue)
        {
            //cron only supports a 4 digit year
            string yearValueString = yearValue.ToString();

            if (yearValueString.Length < 4)
            {
                int fourDigitYear = System.Globalization.CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(yearValue);
                yearValueString = fourDigitYear.ToString();
            }

            return yearValueString;
        }
        /// <summary>
        /// Gets the month cron value string.
        /// </summary>
        /// <param name="monthValue">The month value.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private string GetMonthCronValue(int monthValue)
        {
            string monthString = null;

            switch (monthValue)
            {
                case 1:
                    monthString = "JAN";
                    break;
                case 2:
                    monthString = "FEB";
                    break;
                case 3:
                    monthString = "MAR";
                    break;
                case 4:
                    monthString = "APR";
                    break;
                case 5:
                    monthString = "MAY";
                    break;
                case 6:
                    monthString = "JUN";
                    break;
                case 7:
                    monthString = "JUL";
                    break;
                case 8:
                    monthString = "AUG";
                    break;
                case 9:
                    monthString = "SEP";
                    break;
                case 10:
                    monthString = "OCT";
                    break;
                case 11:
                    monthString = "NOV";
                    break;
                case 12:
                    monthString = "DEC";
                    break;
                default:
                    throw new Exception(string.Format("Invalid Month Value of {0}", monthValue));
                //break;
            }

            return monthString;
        }
        /// <summary>
        /// Gets the ordinal cron value string.
        /// </summary>
        /// <param name="ordinal">The ordinal.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Invalid Ordinal Value</exception>
        private string GetOrdinalCronValue(Ordinal ordinal)
        {
            string ordinalValue = null;

            switch (ordinal)
            {
                case Ordinal.First:
                    ordinalValue = "#1";
                    break;
                case Ordinal.Second:
                    ordinalValue = "#2";
                    break;
                case Ordinal.Third:
                    ordinalValue = "#3";
                    break;
                case Ordinal.Fourth:
                    ordinalValue = "#4";
                    break;
                case Ordinal.Fifth:
                    ordinalValue = "#5";
                    break;
                case Ordinal.Last:
                    ordinalValue = "L";
                    break;
                default:
                    throw new Exception("Invalid Ordinal Value");
                //break;
            }

            return ordinalValue;
        }
        /// <summary>
        /// Gets the day of week cron value string.
        /// </summary>
        /// <param name="dow">The day of week.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Invalid Day Of Week Value</exception>
        private string GetDayOfWeekCronValue(DayOfWeek dow)
        {
            string dowValue = null;

            switch (dow)
            {
                case DayOfWeek.Friday:
                    dowValue = "FRI";
                    break;
                case DayOfWeek.Monday:
                    dowValue = "MON";
                    break;
                case DayOfWeek.Saturday:
                    dowValue = "SAT";
                    break;
                case DayOfWeek.Sunday:
                    dowValue = "SUN";
                    break;
                case DayOfWeek.Thursday:
                    dowValue = "THU";
                    break;
                case DayOfWeek.Tuesday:
                    dowValue = "TUE";
                    break;
                case DayOfWeek.Wednesday:
                    dowValue = "WED";
                    break;
                default:
                    throw new Exception("Invalid Day Of Week Value");
                //break;
            }

            return dowValue;
        }

        #endregion

        #endregion

        #region Schedules Methods

        private IScheduleBuilder CreateScheduleWithAmountAndIntervalUnit(string amountString, string intervalUnitString, TimeZoneInfo timeZone)
        {
            var intervalUnit = GrammarHelper.GetIntervalUnitValueFromString(intervalUnitString);

            int amount = 1;

            if (amountString != null)
                amount = GrammarHelper.GetAmountValueFromString(amountString);
#if CUSTOM
            CustomCalendarIntervalScheduleBuilder b = CustomCalendarIntervalScheduleBuilder.Create();
#else
            CalendarIntervalScheduleBuilder b = CalendarIntervalScheduleBuilder.Create();
#endif

            b.WithInterval(amount, intervalUnit);

            b.PreserveHourOfDayAcrossDaylightSavings(true);
            b.SkipDayIfHourDoesNotExist(false);
            b.InTimeZone(timeZone);
            return b;
        }

        private IScheduleBuilder CreateScheduleWithCron(string cronExpression, TimeZoneInfo timeZone)
        {
            CronScheduleBuilder sb = CronScheduleBuilder.CronSchedule(cronExpression);
            sb.InTimeZone(timeZone);
            return sb;
        }

        #endregion
    }
}
