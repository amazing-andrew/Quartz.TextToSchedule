using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quartz.TextToSchedule.Test
{
    [TestClass]
    public class ValidTests
    {
        private void AssertIsValid(ITextToSchedule tts, string expression)
        {
            Assert.IsTrue(tts.IsValid(expression), "\"{0}\" is not a valid expression.", expression);

            Assert.IsTrue(tts.Parse(expression) != null, "\"{0}\" IS a valid expression, but it does not return any results .", expression);
            Assert.IsTrue(tts.Parse(expression).RegisterGroups.Count > 0, "\"{0}\" IS a valid expression, but it does not return any results .", expression);
        }

        [TestMethod]
        public void ValidExpressions()
        {
            List<string> list = new List<string>()
            {
                "every second", 
                "every 30 minutes", 
                "every 36 hours", 
                "every 12 hours on Monday at 9am", 
                "every 30 minutes on Friday from 9:00 AM to 5:30 PM", 
                "every Friday at 6:30 am", 
                "every mon,wed,fri at 9pm", 
                "2nd,4th Friday of month at 17:00", 
                "first and third Monday of Jan,Feb,Mar", 
                "last day of month at 9:30 am", 
                "3rd Monday of April at 6:00 am and 7:35 pm", 
                "on April 1st at noon", 
                "Jan 1st at midnight", 
                "May 5, 2020 at 4pm", 
                "every Dec 3rd", 
                "March 2nd 3:30 pm", 
                "on May 5th at 5:35 AM and Noon", 
                "every 2 weeks at 08:00", 
                "every 3 days from Jan 3rd, 2012", 
                "every 2 yr from Sep 3 17:00", 
                "every 6 weeks", 
                "every 2 months from February", 
                "every 2 weeks at 6:00am and 7:30am", 
                "every 2 weeks on Monday at 4am", 
                "every 2 weeks on weekends from 12/8/2012 at 9am and 5pm", 
                "every 10th day of month", 
                "every 10th day of month at 6:30 AM", 
                "15th of month at 4:30:30 PM", 
                "15th of January, May, and September at 8:00 AM", 
                "1st day of Feb through Sept", 
                "1st, 10th, and 20th day of month at 8am and 8pm"
            };

            TextToScheduleFactory factory = new TextToScheduleFactory();
            var english = factory.CreateEnglishParser();

            foreach (var expression in list)
            {
                AssertIsValid(english, expression);
            }
        }
    }
}
