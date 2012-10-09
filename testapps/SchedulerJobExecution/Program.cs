using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndrewSmith.Quartz.TextToSchedule;
using AndrewSmith.Quartz.TextToSchedule.Grammars;
using Quartz;

namespace SchedulerJobExecution
{
    class Program
    {
        static void Main(string[] args)
        {
            var schedule = ReadSchedule();

            Quartz.ISchedulerFactory factory = new Quartz.Impl.StdSchedulerFactory();
            var sched = factory.GetScheduler();

            IJobDetail jobDetail = JobBuilder.Create<DummyJob>()
                .Build();

            schedule.ScheduleWithJob(sched, jobDetail);
            sched.Start();
        }

        public static TextToScheduleResults ReadSchedule()
        {
            AndrewSmith.Quartz.TextToSchedule.ITextToSchedule tts = AndrewSmith.Quartz.TextToSchedule.TextToScheduleFactory.CreateEnglishParser();
            //AndrewSmith.TextToSchedule.ITextToSchedule tts = new TextToSchedule(new GermanGrammar(), new GermanGrammarHelper());

            while (true)
            {
                var line = Console.ReadLine();

                if (line != "")
                {
                    try
                    {
                        var schedule = tts.Parse(line);
                        if (schedule.RegisterGroups.Count > 0)
                            return schedule;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                }
            }
        }
    }

    class DummyJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine(DateTime.Now.ToString("dddd, MM:dd:yyyy hh:mm:ss tt"));
        }
    }
}
