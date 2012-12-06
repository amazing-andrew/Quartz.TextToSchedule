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
    }
}
