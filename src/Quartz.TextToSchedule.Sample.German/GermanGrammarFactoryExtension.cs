using Quartz.TextToSchedule.Grammars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quartz.TextToSchedule
{
    public static class GermanGrammarFactoryExtension
    {
        public static ITextToSchedule CreateGermanParser(this TextToScheduleFactory factory)
        {
            return new TextToSchedule(
                new GermanGrammar(),
                new GermanGrammarHelper());
        }
    }
}
