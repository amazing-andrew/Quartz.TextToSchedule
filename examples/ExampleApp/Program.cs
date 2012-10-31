using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewSmith.Quartz.TextToSchedule;
using Quartz;
using Quartz.Impl;

namespace ExampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //create and parse the text into a TextToScheduleResults
            string scheduleText = "every 2 seconds";
            Console.WriteLine("Running schedule of \"{0}\"", scheduleText);

            ITextToSchedule parser = TextToScheduleFactory.CreateEnglishParser();
            TextToScheduleResults results = parser.Parse(scheduleText);

            //create & start the scheduler
            ISchedulerFactory factory = new StdSchedulerFactory();
            IScheduler sched = factory.GetScheduler();
            sched.Start();
            
            //create a job detail
            IJobDetail job = JobBuilder.Create<SampleJob>().Build();

            //schedule job
            results.ScheduleWithJob(sched, job);

            Console.WriteLine("Job scheduled!");
            Console.WriteLine();
        }
    }
}
