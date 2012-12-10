using Quartz.TextToSchedule.Grammars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.TextToSchedule
{
    /// <summary>
    /// A factory class to build <see cref="ITextToSchedule"/> instances.
    /// </summary>
    public class TextToScheduleFactory
    {
        /// <summary>
        /// Creates an <see cref="ITextToSchedule"/> that parses English.
        /// </summary>
        /// <returns></returns>
        public ITextToSchedule CreateEnglishParser()
        {
            return new TextToSchedule(new EnglishGrammar(), new EnglishGrammarHelper());
        }

        /// <summary>
        /// Creates an <see cref="ITextToSchedule"/> that parses Quartz cron expressions.
        /// </summary>
        /// <returns></returns>
        public ITextToSchedule CreateCronParser()
        {
            return new CronTextToSchedule();
        }

        /// <summary>
        /// Creates the standard parser that can understand both English and Cron syntax.
        /// </summary>
        /// <returns></returns>
        public ITextToSchedule CreateStandardParser()
        {
            return new MultiTextToSchedule(
                CreateEnglishParser(),
                CreateCronParser());
        }
    }
}
