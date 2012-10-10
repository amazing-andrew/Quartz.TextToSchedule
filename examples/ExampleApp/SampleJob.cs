using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;

namespace ExampleApp
{
    public class SampleJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("{0}: {1} executed", 
                DateTime.Now.ToString("dddd MM/dd/yyyy hh:mm:ss tt"),
                GetType().FullName);
        }
    }
}
