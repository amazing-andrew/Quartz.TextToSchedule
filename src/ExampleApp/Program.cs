using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz.TextToSchedule;
using Quartz;
using Quartz.Impl;

namespace ExampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //create the text for the schedule
            string scheduleText = "every 2 seconds";
            Console.WriteLine("Running schedule of \"{0}\"", scheduleText);

            //create & start the scheduler
            IScheduler sched = StdSchedulerFactory.GetDefaultScheduler();
            sched.Start();

            //create a job detail
            IJobDetail job = JobBuilder.Create<SampleJob>().Build();

            //schedule job using the extension method
            sched.ScheduleJob(job, scheduleText);

            Console.WriteLine("Job scheduled!");
            Console.WriteLine();
        }
    }
}
