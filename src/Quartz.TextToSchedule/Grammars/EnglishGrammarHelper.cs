using Quartz.TextToSchedule.Util;
using Quartz.Impl.Calendar;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Quartz.TextToSchedule.Grammars
{
    public class EnglishGrammarHelper : IGrammarHelper
    {
        #region Normalizing

        /// <summary>
        /// Normalizes the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public string Normalize(string text)
        {
            //normalize the string
            text = text.ToLower().Trim();
            text = ReplaceSpecialStrings(text);
            text = NormalizeDayOfWeekAndMonthNames(text);
            text = NormalizeRangedValues(text);
            text = ReplaceNoiseWords(text);
            text = NormalizeExtraSpaces(text);
            return text;
        }

        private string ReplaceNoiseWords(string s)
        {
            s = Regex.Replace(s, @"\bthe\b", "");
            s = Regex.Replace(s, @"\ba\b", "");
            s = Regex.Replace(s, @"\ball\b", "every");
            s = Regex.Replace(s, @"\balso\b", "and");
            return s;
        }

        private string ReplaceSpecialStrings(string s)
        {
            s = Regex.Replace(s, @"\bnoon\b", "12:00");
            s = Regex.Replace(s, @"\bmidnight\b", "00:00");
            s = Regex.Replace(s, @"\bweekdays?\b", "mon-fri");
            s = Regex.Replace(s, @"\bweekends?\b", "sat,sun");

            //google app engine synchronized
            s = Regex.Replace(s, @"\bsynchronized\b", "at 00:00");

            s = Regex.Replace(s, @"\beveryday\b", "every day");

            return s;
        }
        private string NormalizeDayOfWeekAndMonthNames(string s)
        {
            s = Regex.Replace(s, @"\b" + EnglishGrammar.MONDAY + @"\b", "mon");
            s = Regex.Replace(s, @"\b" + EnglishGrammar.TUESDAY + @"\b", "tue");
            s = Regex.Replace(s, @"\b" + EnglishGrammar.WEDNESDAY + @"\b", "wed");
            s = Regex.Replace(s, @"\b" + EnglishGrammar.THURSDAY + @"\b", "thu");
            s = Regex.Replace(s, @"\b" + EnglishGrammar.FRIDAY + @"\b", "fri");
            s = Regex.Replace(s, @"\b" + EnglishGrammar.SATURDAY + @"\b", "sat");
            s = Regex.Replace(s, @"\b" + EnglishGrammar.SUNDAY + @"\b", "sun");

            s = Regex.Replace(s, @"\b" + EnglishGrammar.JANUARY + @"\b", "jan");
            s = Regex.Replace(s, @"\b" + EnglishGrammar.FEBRUARY + @"\b", "feb");
            s = Regex.Replace(s, @"\b" + EnglishGrammar.MARCH + @"\b", "mar");
            s = Regex.Replace(s, @"\b" + EnglishGrammar.APRIL + @"\b", "apr");
            s = Regex.Replace(s, @"\b" + EnglishGrammar.MAY + @"\b", "may");
            s = Regex.Replace(s, @"\b" + EnglishGrammar.JUNE + @"\b", "jun");
            s = Regex.Replace(s, @"\b" + EnglishGrammar.JULY + @"\b", "jul");
            s = Regex.Replace(s, @"\b" + EnglishGrammar.AUGUST + @"\b", "aug");
            s = Regex.Replace(s, @"\b" + EnglishGrammar.SEPTEMBER + @"\b", "sep");
            s = Regex.Replace(s, @"\b" + EnglishGrammar.OCTOBER + @"\b", "oct");
            s = Regex.Replace(s, @"\b" + EnglishGrammar.NOVEMBER + @"\b", "nov");
            s = Regex.Replace(s, @"\b" + EnglishGrammar.DECEMBER + @"\b", "dec");

            return s;
        }
        private string NormalizeRangedValues(string s)
        {
            string normalized = Regex.Replace(s, @"\b" + EnglishGrammar.RANGE_SEPARATOR + @"\b", "-");
            return normalized;
        }
        private string NormalizeExtraSpaces(string s)
        {
            return Regex.Replace(s, "\\ +", " ").Trim();
        }

        #endregion

        /// <summary>
        /// Gets the time value from string.
        /// </summary>
        /// <param name="intervalUnitString">The time value string.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Unknown time value string</exception>
        public IntervalUnit GetIntervalUnitValueFromString(string intervalUnitString)
        {
            if (RegexHelper.IsFullMatch(intervalUnitString, EnglishGrammar.INTERVALUNIT_SECOND))
                return IntervalUnit.Second;
            else if (RegexHelper.IsFullMatch(intervalUnitString, EnglishGrammar.INTERVALUNIT_MINUTE))
                return IntervalUnit.Minute;
            else if (RegexHelper.IsFullMatch(intervalUnitString, EnglishGrammar.INTERVALUNIT_HOUR))
                return  IntervalUnit.Hour;
            else if (RegexHelper.IsFullMatch(intervalUnitString, EnglishGrammar.INTERVALUNIT_DAY))
                return IntervalUnit.Day;
            else if (RegexHelper.IsFullMatch(intervalUnitString, EnglishGrammar.INTERVALUNIT_WEEK))
                return IntervalUnit.Week;
            else if (RegexHelper.IsFullMatch(intervalUnitString, EnglishGrammar.INTERVALUNIT_MONTH))
                return IntervalUnit.Month;
            else if (RegexHelper.IsFullMatch(intervalUnitString, EnglishGrammar.INTERVALUNIT_YEAR))
                return IntervalUnit.Year;

            throw new Exception("Unknown time value string");
        }

        /// <summary>
        /// Gets the amount value from string.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public int GetAmountValueFromString(string amount)
        {
            int i = int.Parse(amount);
            return i;
        }

        /// <summary>
        /// Gets the date time from date spec.
        /// </summary>
        /// <param name="datespec">The datespec.</param>
        /// <returns></returns>
        public DateTime? GetDateTimeFromDateSpec(string datespec)
        {
            if (datespec == null)
                return null;

            var matches = RegexHelper.GetNamedMatches(datespec, "^" + EnglishGrammar.DATE_SPEC + "$");

            if (matches == null)
                return null;

            string monthString = matches["MONTH"];
            string dayString = matches["DAY"];
            string yearString = matches["YEAR"];

            int iMonthValue = GetMonthValue(monthString);
            int iDay = 1;
            
            if (dayString != null)
                iDay = int.Parse(dayString);

            int iYear = DateTime.Now.Year; //current year

            if (yearString != null)
                iYear = GetYearValue(yearString);

            return new DateTime(iYear, iMonthValue, iDay, 0, 0, 0, 0);
        }

        /// <summary>
        /// Gets the time from time string.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">timeString is not a valid time</exception>
        public DateTime? GetTimeFromTimeString(string time)
        {
            if (time == null)
                return null;

            var values = RegexHelper.GetNamedMatches(time, "^" + EnglishGrammar.TIME + "$");

            if (values == null)
                throw new ArgumentException("timeString is not a valid time");

            var hour = values["HOUR"];
            var minute = values["MINUTE"];
            var second = values["SECOND"];
            var meridiem = values["MERIDIEM"];

            if (minute == null)
                minute = "00";
            if (second == null)
                second = "00";

            int iHour = int.Parse(hour);
            int iMin = int.Parse(minute);
            int iSec = int.Parse(second);

            if (meridiem != null && RegexHelper.IsFullMatch(meridiem, EnglishGrammar.TIME_PM) && iHour < 12)
            {
                iHour += 12;
            }

            //adjust 12 am to 0
            if (meridiem != null && RegexHelper.IsFullMatch(meridiem, EnglishGrammar.TIME_AM) && iHour == 12)
            {
                iHour = 0;
            }

            DateTime now = DateTime.Now;
            DateTime date = new DateTime(now.Year, now.Month, now.Day, iHour, iMin, iSec);
            return date;
        }

        /// <summary>
        /// Gets the date time from date spec and time.
        /// </summary>
        /// <param name="datespec">The datespec.</param>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public DateTime? GetDateTimeFromDateSpecAndTime(string datespec, string time)
        {
            DateTime? date = GetDateTimeFromDateSpec(datespec);
            DateTime? t = GetTimeFromTimeString(time);

            if (date == null && t == null)
                return null;

            if (date != null && t == null)
                return date;

            if (date == null && t != null)
                return t;

            //both are not null
            return new DateTime(date.Value.Year,
                date.Value.Month,
                date.Value.Day,
                t.Value.Hour,
                t.Value.Minute,
                t.Value.Second);
        }


        /// <summary>
        /// Gets the day of week from string.
        /// </summary>
        /// <param name="dayOfWeekString">The day of week string.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Invalid Day Of Week String</exception>
        public DayOfWeek GetDayOfWeekFromString(string dayOfWeekString)
        {
            if (RegexHelper.IsFullMatch(dayOfWeekString, EnglishGrammar.MONDAY))
                return DayOfWeek.Monday;
            if (RegexHelper.IsFullMatch(dayOfWeekString, EnglishGrammar.TUESDAY))
                return DayOfWeek.Tuesday;
            if (RegexHelper.IsFullMatch(dayOfWeekString, EnglishGrammar.WEDNESDAY))
                return DayOfWeek.Wednesday;
            if (RegexHelper.IsFullMatch(dayOfWeekString, EnglishGrammar.THURSDAY))
                return DayOfWeek.Thursday;
            if (RegexHelper.IsFullMatch(dayOfWeekString, EnglishGrammar.FRIDAY))
                return DayOfWeek.Friday;
            if (RegexHelper.IsFullMatch(dayOfWeekString, EnglishGrammar.SATURDAY))
                return DayOfWeek.Saturday;
            if (RegexHelper.IsFullMatch(dayOfWeekString, EnglishGrammar.SUNDAY))
                return DayOfWeek.Sunday;

            throw new Exception("Invalid Day Of Week String");
        }

        /// <summary>
        /// Gets the day of week values.
        /// </summary>
        /// <param name="dayOfWeekSpecs">The day of week specs.</param>
        /// <returns></returns>
        public List<DayOfWeek> GetDayOfWeekValues(string[] dayOfWeekSpecs)
        {
            List<DayOfWeek> results = new List<DayOfWeek>();

             //each day of week value can be a single value or a ranged value
            foreach (var item in dayOfWeekSpecs)
            {
                //determine if this is a ranged value
                if (RegexHelper.IsFullMatch(item, EnglishGrammar.DAYOFWEEK_RANGE))
                {
                    //this is a range pull the rangestart and rangeend
                    var rangeMatch = RegexHelper.GetNamedMatches(item, "^" + EnglishGrammar.DAYOFWEEK_RANGE + "$");
                    string rangeStart = rangeMatch["RANGESTART"];
                    string rangeEnd   = rangeMatch["RANGEEND"];

                    DayOfWeek rangeStartValue = GetDayOfWeekFromString(rangeStart);
                    DayOfWeek rangeEndValue = GetDayOfWeekFromString(rangeEnd);
                    AddDayOfWeekRangeToList(rangeStartValue, rangeEndValue, results);
                }

                //is normal single day
                else
                {
                    DayOfWeek dow = GetDayOfWeekFromString(item);
                    results.Add(dow);
                }
            }

            return results;
        }

        private void AddDayOfWeekRangeToList(DayOfWeek rangeStartValue, DayOfWeek rangeEndValue, List<DayOfWeek> results)
        {
            int iStart = (int)rangeStartValue;
            int iEnd = (int)rangeEndValue;

            if (iEnd < iStart)
            {
                if (iEnd + 1 == iStart)
                {
                    for (int i = 0; i <= 6; i++)
                    {
                        results.Add((DayOfWeek)i);
                    }
                }
                else
                {
                    int i = iStart;

                    while (i != iEnd + 1)
                    {
                        DayOfWeek dow = (DayOfWeek)i;
                        results.Add(dow);

                        i++;
                        if (i >= 7)
                            i = i - 7;
                    }
                }
            }
            else
            {
                for (int i = iStart; i < iEnd + 1; i++)
                {
                    DayOfWeek dow = (DayOfWeek)i;
                    results.Add(dow);
                }
            }
        }


        /// <summary>
        /// Gets the month values.
        /// </summary>
        /// <param name="monthSpecs">The month specs.</param>
        /// <returns></returns>
        public List<int> GetMonthValues(string[] monthSpecs)
        {
            List<int> results = new List<int>();

            //each day of week value can be a single value or a ranged value
            foreach (var item in monthSpecs)
            {
                //determine if this is a ranged value
                if (RegexHelper.IsFullMatch(item, EnglishGrammar.MONTH_RANGE))
                {
                    //this is a range pull the rangestart and rangeend
                    var rangeMatch = RegexHelper.GetNamedMatches(item, "^" + EnglishGrammar.MONTH_RANGE + "$");
                    string rangeStart = rangeMatch["RANGESTART"];
                    string rangeEnd = rangeMatch["RANGEEND"];

                    int rangeStartValue = GetMonthValue(rangeStart);
                    int rangeEndValue = GetMonthValue(rangeEnd);
                    AddMonthRangeToList(rangeStartValue, rangeEndValue, results);
                }

                //is normal single day
                else
                {
                    int m = GetMonthValue(item);
                    results.Add(m);
                }
            }

            return results;
        }

        private void AddMonthRangeToList(int rangeStartValue, int rangeEndValue, List<int> results)
        {
            int maxMonths = 12;

            int iStart = rangeStartValue;
            int iEnd = rangeEndValue;

            if (iEnd < iStart)
            {
                if (iEnd + 1 == iStart)
                {
                    for (int i = 1; i < maxMonths + 1; i++)
                    {
                        results.Add(i);
                    }
                }
                else
                {
                    int i = iStart;

                    while (i != iEnd + 1)
                    {
                        results.Add(i);

                        i++;
                        if (i > maxMonths)
                            i = i - maxMonths;
                    }
                }
            }
            else
            {
                for (int i = iStart; i < iEnd + 1; i++)
                {
                    results.Add(i);
                }
            }
        }


        //ORDINALS

        /// <summary>
        /// Gets the ordinal values.
        /// </summary>
        /// <param name="ordinals">The ordinals.</param>
        /// <returns></returns>
        public List<Ordinal> GetOrdinalValues(string[] ordinals)
        {
            List<Ordinal> list = new List<Ordinal>();
            foreach (var o in ordinals)
            {
                list.Add(GetOrdinalValue(o));
            }
            return list;
        }

        /// <summary>
        /// Gets the ordinal value.
        /// </summary>
        /// <param name="ordinal">The ordinal.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public Ordinal GetOrdinalValue(string ordinal)
        {
            if (RegexHelper.IsFullMatch(ordinal, EnglishGrammar.ORDINAL_FIRST))
                return Ordinal.First;
            if (RegexHelper.IsFullMatch(ordinal, EnglishGrammar.ORDINAL_SECOND))
                return Ordinal.Second;
            if (RegexHelper.IsFullMatch(ordinal, EnglishGrammar.ORDINAL_THIRD))
                return Ordinal.Third;
            if (RegexHelper.IsFullMatch(ordinal, EnglishGrammar.ORDINAL_FOURTH))
                return Ordinal.Fourth;
            if (RegexHelper.IsFullMatch(ordinal, EnglishGrammar.ORDINAL_FIFTH))
                return Ordinal.Fifth;
            if (RegexHelper.IsFullMatch(ordinal, EnglishGrammar.ORDINAL_LAST))
                return Ordinal.Last;

            throw new Exception(string.Format("UNKNOWN ORDINAL VALUE of {0}", ordinal));

        }

        #region Helper Methods

        public int GetMonthValue(string monthName)
        {
            //check to see if this is a numeric month
            int iMonth = 0;
            if (int.TryParse(monthName, out iMonth))
            {
                if (iMonth >= 1 && iMonth <= 12)
                    return iMonth;
            }

            if (RegexHelper.IsFullMatch(monthName, EnglishGrammar.JANUARY))
                return 1;
            if (RegexHelper.IsFullMatch(monthName, EnglishGrammar.FEBRUARY))
                return 2;
            if (RegexHelper.IsFullMatch(monthName, EnglishGrammar.MARCH))
                return 3;
            if (RegexHelper.IsFullMatch(monthName, EnglishGrammar.APRIL))
                return 4;
            if (RegexHelper.IsFullMatch(monthName, EnglishGrammar.MAY))
                return 5;
            if (RegexHelper.IsFullMatch(monthName, EnglishGrammar.JUNE))
                return 6;
            if (RegexHelper.IsFullMatch(monthName, EnglishGrammar.JULY))
                return 7;
            if (RegexHelper.IsFullMatch(monthName, EnglishGrammar.AUGUST))
                return 8;
            if (RegexHelper.IsFullMatch(monthName, EnglishGrammar.SEPTEMBER))
                return 9;
            if (RegexHelper.IsFullMatch(monthName, EnglishGrammar.OCTOBER))
                return 10;
            if (RegexHelper.IsFullMatch(monthName, EnglishGrammar.NOVEMBER))
                return 11;
            if (RegexHelper.IsFullMatch(monthName, EnglishGrammar.DECEMBER))
                return 12;

            throw new Exception("Invalid month value");
        }

        #endregion


        /// <summary>
        /// Gets an individual year value.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        public int GetYearValue(string year)
        {
            if (year.Length == 2)
                return CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(int.Parse(year));
            else
                return int.Parse(year);
        }


        /// <summary>
        /// Gets the day value.
        /// </summary>
        /// <param name="day">The day.</param>
        /// <returns></returns>
        public int GetDayValue(string day)
        {
            return int.Parse(day);
        }
    }
}
