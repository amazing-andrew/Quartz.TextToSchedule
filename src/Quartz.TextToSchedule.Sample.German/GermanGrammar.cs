using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Quartz.TextToSchedule.Sample.German;
using Quartz.TextToSchedule.Util;

namespace Quartz.TextToSchedule.Grammars
{
    //warning: I do not know the German language.
    //I put most of this together playing with online translators

    public class GermanGrammar : IGrammar
    {
        //public static readonly string SPACE = @"\ ";
        public static readonly string LIST_SEPARATOR = @"(,|;|\/|\\|\ |\ und\ )";
        public static readonly string RANGE_SEPARATOR = "( durch | ?- ?| bis )";
        public static readonly string AMOUNT = @"(?<AMOUNT>\d+)\.?";

        #region Patterns - INTERVALUNIT

        //INTERVALUNIT
        //regular time value is (seconds|minutes|hours)
        public static readonly string INTERVALUNIT_SECOND = "(s|sek|sekunde|sekunden)";
        public static readonly string INTERVALUNIT_MINUTE = "(m|min|minute|minuten)";
        public static readonly string INTERVALUNIT_HOUR = "(h|std|stunde|stunden)";

        public static readonly string INTERVALUNIT_DAY = "(tag|tags|tage)";
        public static readonly string INTERVALUNIT_WEEK = "(wochen?)";
        public static readonly string INTERVALUNIT_MONTH = "(monate?n?)";
        public static readonly string INTERVALUNIT_YEAR = "(jahr(e|n|en|lich))";

        public static readonly string INTERVALUNIT_TIME = RegexHelper.Builder_GroupOf("INTERVALUNIT", new string[] { INTERVALUNIT_SECOND, INTERVALUNIT_MINUTE, INTERVALUNIT_HOUR });
        public static readonly string INTERVALUNIT_DATE = RegexHelper.Builder_GroupOf("INTERVALUNIT", new string[] { INTERVALUNIT_DAY, INTERVALUNIT_WEEK, INTERVALUNIT_MONTH, INTERVALUNIT_YEAR });

        #endregion

        #region Patterns - DAYOF WEEK

        //DAYOFWEEK
        public static readonly string MONDAY = @"(m|mo|montag)";
        public static readonly string TUESDAY = @"(di|dienstag)";
        public static readonly string WEDNESDAY = @"(mi|mittwoch)";
        public static readonly string THURSDAY = @"(do|donnerstag)";
        public static readonly string FRIDAY = @"(f|fr|freitag)";
        public static readonly string SATURDAY = @"(sa|samstag)";
        public static readonly string SUNDAY = @"(so|sonntag)";

        public static readonly string DAYOFWEEK_ONE = RegexHelper.Builder_GroupOf(new string[] { MONDAY, TUESDAY, WEDNESDAY, THURSDAY, FRIDAY, SATURDAY, SUNDAY });
        public static readonly string DAYOFWEEK_RANGE = RegexHelper.Builder_Range(DAYOFWEEK_ONE, RANGE_SEPARATOR);
        public static readonly string DAYOFWEEK = RegexHelper.Builder_GroupOf("DAYOFWEEK", new string[] { DAYOFWEEK_ONE, DAYOFWEEK_RANGE });

        public static readonly string DAYOFWEEK_SPEC = RegexHelper.Builder_ListOf(DAYOFWEEK, LIST_SEPARATOR);

        #endregion

        #region Patterns - MONTHS

        //MONTHS
        public static readonly string JANUARY = "(jan|jän|januar|january)";
        public static readonly string FEBRUARY = "(feb|februar|february)";
        public static readonly string MARCH = "(mar|märz|marz|marsch)";
        public static readonly string APRIL = "(apr|april)";
        public static readonly string MAY = "(may|mai|may)";
        public static readonly string JUNE = "(jun|juni|june)";
        public static readonly string JULY = "(jul|juli|july)";
        public static readonly string AUGUST = "(aug|august)";
        public static readonly string SEPTEMBER = "(sep|sept|september)";
        public static readonly string OCTOBER = "(oct|okt|october|oktober)";
        public static readonly string NOVEMBER = "(nov|nov|november)";
        public static readonly string DECEMBER = "(dec|dez|december|dezember)";

        public static readonly string MONTH_ONE = RegexHelper.Builder_GroupOf(new string[] { JANUARY, FEBRUARY, MARCH, APRIL, MAY, JUNE, JULY, AUGUST, SEPTEMBER, OCTOBER, NOVEMBER, DECEMBER });
        public static readonly string MONTH_RANGE = RegexHelper.Builder_Range(MONTH_ONE, RANGE_SEPARATOR);
        public static readonly string MONTH = RegexHelper.Builder_GroupOf("MONTH", new string[] { MONTH_ONE, MONTH_RANGE });

        public static readonly string MONTH_SPEC = RegexHelper.Builder_ListOf(MONTH, LIST_SEPARATOR);

        #endregion

        #region Patterns - ORDINAL

        //ORDINAL
        public static readonly string ORDINAL_FIRST = "(erste(r|s|n)?|1\\.?)";
        public static readonly string ORDINAL_SECOND = "(zweite(r|s|n)?|2\\.?)";
        public static readonly string ORDINAL_THIRD = "(dritte(r|s|n)?|3\\.?)";
        public static readonly string ORDINAL_FOURTH = "(vierte(r|s|n)?|4\\.?)";
        public static readonly string ORDINAL_FIFTH = "(f(ü|u)nfte(r|s|n)?|5\\.?)";
        public static readonly string ORDINAL_LAST = "(letzten|vergangenen)";

        public static readonly string ORDINAL_ALL = RegexHelper.Builder_GroupOf("ORDINAL", new string[] { ORDINAL_FIRST, ORDINAL_SECOND, ORDINAL_THIRD, ORDINAL_FOURTH, ORDINAL_FIFTH, ORDINAL_LAST });
        public static readonly string ORDINAL = RegexHelper.Builder_ListOf(ORDINAL_ALL, LIST_SEPARATOR);

        #endregion

        #region Patterns - TIME

        //TIME
        public static readonly string TIME_HOUR = @"(?<HOUR>\d|[0-1]\d|2[0-3])";
        public static readonly string TIME_MINUTE = @"(?<MINUTE>[0-5]\d)";
        public static readonly string TIME_SECOND = @"(?<SECOND>[0-5]\d)";
        //public static readonly string TIME_AM = @"(am|a\.m\.|a\.m|am\.)";
        //public static readonly string TIME_PM = @"(pm|p\.m\.|p\.m|pm\.)";
        //public static readonly string TIME_MERIDIEM = RegexHelper.Builder_GroupOf("MERIDIEM", new string[] { TIME_AM, TIME_PM });

        public static readonly string TIME = @"(?<TIME>{TIME_HOUR}((:|\.| ){TIME_MINUTE})?((:|\.| ){TIME_SECOND})? ?(uhr)?)";

        public static readonly string TIME_ONCE = "(( ?(von|vom|ab|auf|um|im|am)? ?)?{TIME})";
        public static readonly string TIME_RANGE = "((von|vom|ab|auf|um|im|am)? ?(?<FROMTIME>{TIME}) bis (?<TOTIME>{TIME}))";
        public static readonly string TIME_SPEC = RegexHelper.Builder_GroupOf("TIMESPEC", new string[] { TIME_ONCE, TIME_RANGE });

        #endregion

        #region Patterns - DATE

        public static readonly string DATE_DAY = @"(?<DAY>(0?[1-9])|(1[0-9])|(2[0-9])|(3[0-3]))(st|nd|rd|th|\.)?";
        public static readonly string DATE_YEAR = @"(?<YEAR>\d\d(\d\d)?)";
        public static readonly string DATE = @"({MONTH} ?,? ?({DATE_DAY})?( ?,? ({DATE_YEAR}))?)";
        public static readonly string DATE2 = @"({DATE_DAY} ?,? ?{MONTH}( ?,? {DATE_YEAR})?)"; //added another date
        public static readonly string DATE_SPEC = RegexHelper.Builder_GroupOf("DATESPEC", new string[] { DATE, DATE2 });

        #endregion

        
        static GermanGrammar()
        {
            var members = typeof(GermanGrammar).GetMembers();

            foreach (var f in members.OfType<FieldInfo>().Where(x => x.FieldType == typeof(string)))
            {
                f.SetValue(null, NamedFormatHelper.NamedFormat(f.GetValue(null) as string, typeof(GermanGrammar)));
            }
            foreach (var p in members.OfType<PropertyInfo>().Where(x => x.PropertyType == typeof(string)))
            {
                if (p.CanRead && p.CanWrite)
                {
                    p.SetValue(null, NamedFormatHelper.NamedFormat(p.GetValue(null, null) as string, typeof(GermanGrammar)), null);
                }
            }
        }

        #region Expressions

        //every [n] (sec|min|hour) [on mon-fri] [of monthspec] [time]
        public static readonly string SpecialExpr1 = @"((jede|alle|jeden|jeder)( {AMOUNT})? {INTERVALUNIT_TIME}(( (von|vom|im|am|um))? {DAYOFWEEK_SPEC})?(( (von|vom|im|am|um))? {MONTH_SPEC})?( {TIME_SPEC})?)";

        //(every|ordinal) (day of week) [of (month)] [time]
        public static readonly string SpecialExpr2 = @"(((jede|alle|jeden|jeder)|(am )?{ORDINAL})( (tag|tags|{DAYOFWEEK_SPEC}))(( ?(von|vom|im|am|um))? (((des|im|eines?) )?monats|{MONTH_SPEC}))?( {TIME_ONCE})?)";

        //[every|on] (date) [time]
        public static readonly string SpecialExpr3 = @"(((jede|alle|jeden|jeder|am|um) )?{DATE_SPEC}( {TIME_ONCE})?)";

        /// every [n] (days|weeks|months|years) (from [date]) (at [time])
        public static readonly string SpecialExpr4 = @"(((jede|alle|jeden|jeder) )?( ?{AMOUNT})? ?{INTERVALUNIT_DATE}( ?(von|vom|ab|auf|um|im|am)? {DATE_SPEC})?( {TIME_SPEC})?)";


        #endregion

        public string Expression1
        {
            get { return SpecialExpr1; }
        }

        public string Expression2
        {
            get
            {
                return SpecialExpr2;
            }
        }

        public string Expression3
        {
            get { return SpecialExpr3; }
        }


        public string Expression4
        {
            get { return SpecialExpr4; }
        }


        public string Expression5
        {
            get { return "notimplemented"; }
        }
    }
}