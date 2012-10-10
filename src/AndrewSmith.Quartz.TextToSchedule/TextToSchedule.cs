using AndrewSmith.Quartz.TextToSchedule.Calendars;
using AndrewSmith.Quartz.TextToSchedule.Grammars;
using Quartz;
using Quartz.Impl.Calendar;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndrewSmith.Quartz.TextToSchedule
{
    /// <summary>
    /// Default Impl of an <see cref="ITextToSchedule"/>
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
            text = GrammarHelper.Normalize(text);

            TextToScheduleResults results = new TextToScheduleResults();
            bool matched = false;

            if (ExecuteMatch(Grammar.Expression1, text, results, Expression1Handler))
            {
                matched = true;
            }
            else if (ExecuteMatch(Grammar.Expression2, text, results, Expression2Handler))
            {
                matched = true;
            }
            else if (ExecuteMatch(Grammar.Expression3, text, results, Expression3Handler))
            {
                matched = true;
            }


            if (matched)
                return results;   
            else
                return null;
        }

        /// <summary>
        /// Handles with a given text matches the Expression1 field.
        /// </summary>
        /// <param name="nameValueCollection">A collection of values from the named capture groups.</param>
        /// <param name="results">The results.</param>
        private void Expression1Handler(NameValueCollection nameValueCollection, TextToScheduleResults results)
        {
            var amount = nameValueCollection["AMOUNT"];
            var timeValue = nameValueCollection["TIMEVALUE"];
            var startDate = nameValueCollection["DATESPEC"];
            
            var time = nameValueCollection["TIME"];
            var fromTime = nameValueCollection["FROMTIME"];
            var toTime = nameValueCollection["TOTIME"];

            var dayOfWeekSpecs = nameValueCollection.GetValues("DAYOFWEEK");
            var monthSpecs = nameValueCollection.GetValues("MONTH");


            TimeSpan interval = BuildTimeInterval(timeValue, amount);
            DateTime? triggerStartTime = null;

            ICalendar calendar = null;

            //DAY OF WEEK SPECS
            if (dayOfWeekSpecs != null)
            {
                calendar = BuildCalendarOnDayOfWeek(calendar, dayOfWeekSpecs);
            }

            //MONTH SPECS (I had to put this after the day of week, because chaining the calendar's didn't seem to compute properly)
            if (monthSpecs != null)
            {
                calendar = BuildCalendarOnMonths(calendar, monthSpecs);
            }


            //TIME (single or range)

            //check for ranged time
            if (fromTime != null && toTime != null)
            {
                calendar = BuildCalendarOnTimeRange(calendar, fromTime, toTime);

                //set the start date as the from time
                DateTime? fromTimeStartDate = GrammarHelper.GetTimeFromTimeString(fromTime);
                triggerStartTime = fromTimeStartDate;
            }
            //is regualr time, process as single time provided
            else if (time != null)
            {
                DateTime? timeStartDate = GrammarHelper.GetTimeFromTimeString(time);
                triggerStartTime = timeStartDate;
            }
            
            //BUILD TRIGGER
            TriggerBuilder triggerBuilder = TriggerBuilder.Create();

            SimpleScheduleBuilder simpleBuilder = SimpleScheduleBuilder.Create();
            simpleBuilder.WithInterval(interval);
            simpleBuilder.RepeatForever();
            triggerBuilder.WithSchedule(simpleBuilder);
            
            //start on from time
            if (triggerStartTime != null)
                triggerBuilder.StartAt(new DateTimeOffset(triggerStartTime.Value));

            results.Add(triggerBuilder, calendar);
        }

        /// <summary>
        /// Handles with a given text matches the Expression2 field.
        /// </summary>
        /// <param name="nameValueCollection">A collection of values from the named capture groups.</param>
        /// <param name="results">The results.</param>
        private void Expression2Handler(NameValueCollection nameValueCollection, TextToScheduleResults results)
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
        
            foreach (var item in cronExpressions)
            {
                var triggerBuilder = TriggerBuilder.Create();

                CronScheduleBuilder cronScheduleBuilder = CronScheduleBuilder.CronSchedule(item);
                triggerBuilder.WithSchedule(cronScheduleBuilder);

                ITrigger trigger = triggerBuilder.Build();
                results.Add(triggerBuilder);
            }
        }

        /// <summary>
        /// Handles with a given text matches the Expression2 field.
        /// </summary>
        /// <param name="nameValueCollection">A collection of values from the named capture groups.</param>
        /// <param name="results">The results.</param>
        private void Expression3Handler(NameValueCollection nameValueCollection, TextToScheduleResults results)
        {
            var time = nameValueCollection["TIME"];

            var dateSpec = nameValueCollection["DATESPEC"];
            var month = nameValueCollection["MONTH"];
            var day = nameValueCollection["DAY"];
            var year = nameValueCollection["YEAR"];

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

            cron_month = GetMonthCronValue(GrammarHelper.GetMonthValue(month));
            cron_day = day;

            if (year != null)
                cron_year = GetYearCronValue(GrammarHelper.GetYearValue(year));


            //build cron string
            string cronString =  null;

            if (cron_year != null)
                cronString = string.Format(string.Join(" ", new string[] { cron_sec, cron_min, cron_hour, cron_day, cron_month, cron_dayofWeek, cron_year }));
            else
                cronString = string.Format(string.Join(" ", new string[] { cron_sec, cron_min, cron_hour, cron_day, cron_month, cron_dayofWeek }));

            //add cron string
            cronExpressions.Add(cronString);



            foreach (var item in cronExpressions)
            {
                var triggerBuilder = TriggerBuilder.Create();

                CronScheduleBuilder cronScheduleBuilder = CronScheduleBuilder.CronSchedule(item);
                triggerBuilder.WithSchedule(cronScheduleBuilder);

                ITrigger trigger = triggerBuilder.Build();
                results.Add(triggerBuilder);
            }
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
        private static bool ExecuteMatch(string expression, string input, TextToScheduleResults results, Action<NameValueCollection, TextToScheduleResults> matchFunction)
        {
            expression = "^" + expression + "$";

            var dic = RegexHelper.GetNamedMatches(input, expression);

            if (dic != null)
            {
                matchFunction(dic, results);
                return true;
            }

            return false;                
        }

        #endregion

        #region Calendar & Helper Methods

        /// <summary>
        /// Builds the time interval based on a time value and amount fields.
        /// </summary>
        /// <param name="timeValueString">The time value string.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        private TimeSpan BuildTimeInterval(string timeValueString, string amount)
        {
            TimeValue timeValue = GrammarHelper.GetTimeValueFromString(timeValueString);
            int iAmount = amount == null ? 1 : GrammarHelper.GetAmountValueFromString(amount); 
            TimeSpan interval = new TimeSpan();

            switch (timeValue)
            {
                case TimeValue.Seconds:
                    interval = TimeSpan.FromSeconds(iAmount);
                    break;
                case TimeValue.Minutes:
                    interval = TimeSpan.FromMinutes(iAmount);
                    break;
                case TimeValue.Hours:
                    interval = TimeSpan.FromHours(iAmount);
                    break;
                default:
                    break;
            }

            return interval;
        }
        
        /// <summary>
        /// Builds a <see cref="WeeklyCalendar"/> based on the given allowed days of weeks.
        /// </summary>
        /// <param name="dayofWeekSpecs">The day of week specs.</param>
        /// <returns></returns>
        private WeeklyCalendar BuildCalendarOnDayOfWeek(ICalendar baseCalendar, string[] dayofWeekSpecs)
        {
            //create calendar and exclude all days
            //WeeklyCalendar calendar = new WeeklyCalendar();
            LocalWeeklyCalendar calendar = null;

            if (baseCalendar != null)
                calendar = new LocalWeeklyCalendar(baseCalendar);
            else
                calendar = new LocalWeeklyCalendar();

            calendar.DaysExcluded = new bool[7] { true, true, true, true, true, true, true};

            var dayOfWeeks = GrammarHelper.GetDayOfWeekValues(dayofWeekSpecs);

            foreach (var item in dayOfWeeks)
            {
                calendar.SetDayExcluded(item, false);
            }
            
            return calendar;
        }

        /// <summary>
        /// Builds a <see cref="DailyCalendar"/> on the given allowed hours to run.
        /// </summary>
        /// <param name="fromTime">From time.</param>
        /// <param name="toTime">To time.</param>
        /// <returns></returns>
        private DailyCalendar BuildCalendarOnTimeRange(ICalendar baseCalendar, string fromTime, string toTime)
        {
            DateTime? dFromTime = GrammarHelper.GetTimeFromTimeString(fromTime);
            DateTime? dToTime = GrammarHelper.GetTimeFromTimeString(toTime);

            DateTime fromUtc = dFromTime.Value.ToUniversalTime();
            DateTime toUtc = dToTime.Value.ToUniversalTime();
            
            //adjust the utc month,day,year to match each other
            toUtc = new DateTime(fromUtc.Year, fromUtc.Month, fromUtc.Day, toUtc.Hour, toUtc.Minute, toUtc.Second, toUtc.Millisecond);

            DailyCalendar calendar = null;

            //if the to time is lower than from
            if (toUtc < fromUtc)
            {
                //switch the variables
                if (baseCalendar != null)
                    calendar = new DailyCalendar(baseCalendar, toUtc, fromUtc);
                else
                    calendar = new DailyCalendar(toUtc, fromUtc);
            }
            else
            {
                //check to see if they are the same
                //TODO: do something about this hacking the extra second
                if (fromUtc.Equals(toUtc))
                {
                    toUtc = toUtc.AddSeconds(1); 
                }

                if (baseCalendar != null)
                    calendar = new DailyCalendar(baseCalendar, fromUtc, toUtc);
                else
                    calendar = new DailyCalendar(fromUtc, toUtc);

                calendar.InvertTimeRange = true; //turn this into an inclusive range
            }

            return calendar;
        }

        /// <summary>
        /// Builds a <see cref="ICalendar"/> based on the allowed months.
        /// </summary>
        /// <param name="monthSpecs">The month specs.</param>
        /// <returns></returns>
        private ICalendar BuildCalendarOnMonths(ICalendar baseCalendar, string[] monthSpecs)
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

            return cronCal;
        }

        #region Cron Value Strings

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
    }
}
