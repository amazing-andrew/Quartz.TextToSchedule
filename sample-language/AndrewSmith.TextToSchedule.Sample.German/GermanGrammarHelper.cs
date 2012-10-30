using Quartz.Impl.Calendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AndrewSmith.Quartz.TextToSchedule.Sample.German;

namespace AndrewSmith.Quartz.TextToSchedule.Grammars
{
    public class GermanGrammarHelper : IGrammarHelper
    {
        #region Normalizing

        public string Normalize(string s)
        {
            //normalize the string
            s = s.ToLower().Trim();
            s = NormalizeExtraSpaces(s);
            s = ReplaceSpecialStrings(s);
            //s = NormalizeDayOfWeekAndMonthNames(s);
            //s = NormalizeRangedValues(s);

            return s;
        }

        private string ReplaceSpecialStrings(string s)
        {
            s = Regex.Replace(s, @"\bmittag\b", "12.00");
            s = Regex.Replace(s, @"\bmitternacht\b", "00.00");
            s = Regex.Replace(s, @"\b(wochentag|werktags)\b", "montag-freitag");
            s = Regex.Replace(s, @"\bwochenende\b", "samstag,sonntag");

            //google app engine synchronized
            s = Regex.Replace(s, @"\b(synchronized|synchronisiert|zeitgleich)\b", "00.00");
            return s;
        }
        private string NormalizeDayOfWeekAndMonthNames(string s)
        {
            //s = Regex.Replace(s, @"\b" + GermanGrammar.MONDAY + @"\b", "mon");
            //s = Regex.Replace(s, @"\b" + GermanGrammar.TUESDAY + @"\b", "tue");
            //s = Regex.Replace(s, @"\b" + GermanGrammar.WEDNESDAY + @"\b", "wed");
            //s = Regex.Replace(s, @"\b" + GermanGrammar.THURSDAY + @"\b", "thu");
            //s = Regex.Replace(s, @"\b" + GermanGrammar.FRIDAY + @"\b", "fri");
            //s = Regex.Replace(s, @"\b" + GermanGrammar.SATURDAY + @"\b", "sat");
            //s = Regex.Replace(s, @"\b" + GermanGrammar.SUNDAY + @"\b", "sun");

            //s = Regex.Replace(s, @"\b" + GermanGrammar.JANUARY + @"\b", "jan");
            //s = Regex.Replace(s, @"\b" + GermanGrammar.FEBRUARY + @"\b", "feb");
            //s = Regex.Replace(s, @"\b" + GermanGrammar.MARCH + @"\b", "mar");
            //s = Regex.Replace(s, @"\b" + GermanGrammar.APRIL + @"\b", "apr");
            //s = Regex.Replace(s, @"\b" + GermanGrammar.MAY + @"\b", "may");
            //s = Regex.Replace(s, @"\b" + GermanGrammar.JUNE + @"\b", "jun");
            //s = Regex.Replace(s, @"\b" + GermanGrammar.JULY + @"\b", "jul");
            //s = Regex.Replace(s, @"\b" + GermanGrammar.AUGUST + @"\b", "aug");
            //s = Regex.Replace(s, @"\b" + GermanGrammar.SEPTEMBER + @"\b", "sep");
            //s = Regex.Replace(s, @"\b" + GermanGrammar.OCTOBER + @"\b", "oct");
            //s = Regex.Replace(s, @"\b" + GermanGrammar.NOVEMBER + @"\b", "nov");
            //s = Regex.Replace(s, @"\b" + GermanGrammar.DECEMBER + @"\b", "dec");

            return s;
        }
        private string NormalizeRangedValues(string s)
        {
            string normalized = Regex.Replace(s, @"\b" + GermanGrammar.RANGE_SEPARATOR + @"\b", "-");
            return normalized;
        }
        private string NormalizeExtraSpaces(string s)
        {
            return Regex.Replace(s, "\\ +", " ");
        }

        #endregion

        public global::Quartz.IntervalUnit GetIntervalUnitValueFromString(string intervalUnitString)
        {
            if (RegexHelper.IsFullMatch(intervalUnitString, GermanGrammar.INTERVALUNIT_SECOND))
                return global::Quartz.IntervalUnit.Second;
            else if (RegexHelper.IsFullMatch(intervalUnitString, GermanGrammar.INTERVALUNIT_MINUTE))
                return global::Quartz.IntervalUnit.Minute;
            else if (RegexHelper.IsFullMatch(intervalUnitString, GermanGrammar.INTERVALUNIT_HOUR))
                return global::Quartz.IntervalUnit.Hour;

            throw new Exception("Unknown time value string");
        }
        public int GetAmountValueFromString(string amount)
        {
            int i = int.Parse(amount);
            return i;
        }

        public DateTime? GetDateTimeFromDateSpec(string datespec)
        {
            if (datespec == null)
                return null;

            var matches = RegexHelper.GetNamedMatches(datespec, GermanGrammar.DATE_SPEC);

            if (matches == null)
                return null;

            string month = matches["MONTH"];
            string day = matches["DAY"];

            int iMonthValue = GetMonthValue(month);
            int iDay = int.Parse(day);

            int currentYear = DateTime.Now.Year;
            return new DateTime(currentYear, iMonthValue, iDay);
        }
        public DateTime? GetTimeFromTimeString(string time)
        {
            if (time == null)
                return null;

            var values = RegexHelper.GetNamedMatches(time, "^" + GermanGrammar.TIME + "$");

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

            //if (meridiem != null && RegexHelper.IsFullMatch(meridiem, GermanGrammar.TIME_PM) && iHour < 12)
            //{
            //    iHour += 12;
            //}

            ////adjust 12 am to 0
            //if (meridiem != null && RegexHelper.IsFullMatch(meridiem, GermanGrammar.TIME_AM) && iHour == 12)
            //{
            //    iHour = 0;
            //}

            DateTime now = DateTime.Now;
            DateTime date = new DateTime(now.Year, now.Month, now.Day, iHour, iMin, iSec);
            return date;
        }

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


        public DayOfWeek GetDayOfWeekFromString(string dayOfWeekString)
        {
            if (RegexHelper.IsFullMatch(dayOfWeekString, GermanGrammar.MONDAY))
                return DayOfWeek.Monday;
            if (RegexHelper.IsFullMatch(dayOfWeekString, GermanGrammar.TUESDAY))
                return DayOfWeek.Tuesday;
            if (RegexHelper.IsFullMatch(dayOfWeekString, GermanGrammar.WEDNESDAY))
                return DayOfWeek.Wednesday;
            if (RegexHelper.IsFullMatch(dayOfWeekString, GermanGrammar.THURSDAY))
                return DayOfWeek.Thursday;
            if (RegexHelper.IsFullMatch(dayOfWeekString, GermanGrammar.FRIDAY))
                return DayOfWeek.Friday;
            if (RegexHelper.IsFullMatch(dayOfWeekString, GermanGrammar.SATURDAY))
                return DayOfWeek.Saturday;
            if (RegexHelper.IsFullMatch(dayOfWeekString, GermanGrammar.SUNDAY))
                return DayOfWeek.Sunday;

            throw new Exception("Invalid Day Of Week String");
        }

        public List<DayOfWeek> GetDayOfWeekValues(string[] dayOfWeekSpecs)
        {
            List<DayOfWeek> results = new List<DayOfWeek>();

             //each day of week value can be a single value or a ranged value
            foreach (var item in dayOfWeekSpecs)
            {
                //determine if this is a ranged value
                if (RegexHelper.IsFullMatch(item, GermanGrammar.DAYOFWEEK_RANGE))
                {
                    //this is a range pull the rangestart and rangeend
                    var rangeMatch = RegexHelper.GetNamedMatches(item, "^" + GermanGrammar.DAYOFWEEK_RANGE + "$");
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
        /// Gets the month value.
        /// </summary>
        /// <param name="monthName">Name of the month.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Invalid month value</exception>
        public int GetMonthValue(string monthName)
        {
            if (RegexHelper.IsFullMatch(monthName, GermanGrammar.JANUARY))
                return 1;
            if (RegexHelper.IsFullMatch(monthName, GermanGrammar.FEBRUARY))
                return 2;
            if (RegexHelper.IsFullMatch(monthName, GermanGrammar.MARCH))
                return 3;
            if (RegexHelper.IsFullMatch(monthName, GermanGrammar.APRIL))
                return 4;
            if (RegexHelper.IsFullMatch(monthName, GermanGrammar.MAY))
                return 5;
            if (RegexHelper.IsFullMatch(monthName, GermanGrammar.JUNE))
                return 6;
            if (RegexHelper.IsFullMatch(monthName, GermanGrammar.JULY))
                return 7;
            if (RegexHelper.IsFullMatch(monthName, GermanGrammar.AUGUST))
                return 8;
            if (RegexHelper.IsFullMatch(monthName, GermanGrammar.SEPTEMBER))
                return 9;
            if (RegexHelper.IsFullMatch(monthName, GermanGrammar.OCTOBER))
                return 10;
            if (RegexHelper.IsFullMatch(monthName, GermanGrammar.NOVEMBER))
                return 11;
            if (RegexHelper.IsFullMatch(monthName, GermanGrammar.DECEMBER))
                return 12;

            throw new Exception("Invalid month value");
        }

        public List<int> GetMonthValues(string[] monthSpecs)
        {
            List<int> results = new List<int>();

            //each day of week value can be a single value or a ranged value
            foreach (var item in monthSpecs)
            {
                //determine if this is a ranged value
                if (RegexHelper.IsFullMatch(item, GermanGrammar.MONTH_RANGE))
                {
                    //this is a range pull the rangestart and rangeend
                    var rangeMatch = RegexHelper.GetNamedMatches(item, "^" + GermanGrammar.MONTH_RANGE + "$");
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
                int i = iStart;

                while (i != iEnd + 1)
                {
                    results.Add(i);

                    i++;
                    if (i >= maxMonths)
                        i = i - maxMonths;
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

        public List<Ordinal> GetOrdinalValues(string[] ordinals)
        {
            List<Ordinal> list = new List<Ordinal>();
            foreach (var o in ordinals)
            {
                list.Add(GetOrdinalValue(o));
            }
            return list;
        }

        public Ordinal GetOrdinalValue(string ordinal)
        {
            if (RegexHelper.IsFullMatch(ordinal, GermanGrammar.ORDINAL_FIRST))
                return Ordinal.First;
            if (RegexHelper.IsFullMatch(ordinal, GermanGrammar.ORDINAL_SECOND))
                return Ordinal.Second;
            if (RegexHelper.IsFullMatch(ordinal, GermanGrammar.ORDINAL_THIRD))
                return Ordinal.Third;
            if (RegexHelper.IsFullMatch(ordinal, GermanGrammar.ORDINAL_FOURTH))
                return Ordinal.Fourth;
            if (RegexHelper.IsFullMatch(ordinal, GermanGrammar.ORDINAL_FIFTH))
                return Ordinal.Fifth;
            if (RegexHelper.IsFullMatch(ordinal, GermanGrammar.ORDINAL_LAST))
                return Ordinal.Last;

            throw new Exception(string.Format("UNKNOWN ORDINAL VALUE of {0}", ordinal));

        }


        public int GetYearValue(string year)
        {
            return int.Parse(year);
        }
    }
}
