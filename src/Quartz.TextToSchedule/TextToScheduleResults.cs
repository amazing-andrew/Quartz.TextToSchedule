using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.TextToSchedule
{
    /// <summary>
    /// The results of a <see cref="ITextToSchedule.Parse"/>. 
    /// You can register the given schedule with methods defined in the class.
    /// </summary>
    public class TextToScheduleResults
    {
        /// <summary>
        /// Gets the groups of paired Triggers and Calendars.
        /// </summary>
        public List<RegisterGroup> RegisterGroups { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextToScheduleResults" /> class.
        /// </summary>
        public TextToScheduleResults()
        {
            RegisterGroups = new List<RegisterGroup>();
        }

        /// <summary>
        /// Adds the specified trigger builder.
        /// </summary>
        /// <param name="triggerBuilder">The trigger builder.</param>
        public void Add(TriggerBuilder triggerBuilder)
        {
            RegisterGroups.Add(new RegisterGroup(triggerBuilder));
        }

        /// <summary>
        /// Adds the specified trigger and calendar as a pair.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        /// <param name="calendar">The calendar.</param>
        public void Add(TriggerBuilder trigger, ICalendar calendar)
        {
            RegisterGroups.Add(new RegisterGroup(trigger, calendar));
        }

        #region Registering With Scheduler

        /// <summary>
        /// Registers the calendars to the scheduler.
        /// </summary>
        /// <param name="sched">The sched.</param>
        private void RegisterCalendars(IScheduler sched)
        {
            foreach (var g in RegisterGroups)
            {
                ICalendar cal = g.Calendar;

                if (cal != null)
                {
                    string newCalendarName = string.Format("CAL_{0}", Guid.NewGuid());
                    sched.AddCalendar(newCalendarName, cal, true, true);

                    g.TriggerBuilder.ModifiedByCalendar(newCalendarName);
                }
            }
        }

        /// <summary>
        /// Schedules the Triggers with a given job key.
        /// </summary>
        /// <param name="sched">The sched.</param>
        /// <param name="jobKey">The job key.</param>
        public void ScheduleWithJobKey(IScheduler sched, JobKey jobKey)
        {
            RegisterCalendars(sched);

            foreach (var group in RegisterGroups)
            {
                group.TriggerBuilder.ForJob(jobKey);

                ITrigger trigger = group.TriggerBuilder.Build();
                sched.ScheduleJob(trigger);
            }
        }

        /// <summary>
        /// Schedules the with the provided <see cref="IJobDetail"/>.
        /// </summary>
        /// <param name="sched">The sched.</param>
        /// <param name="jobDetail">The job detail.</param>
        public void ScheduleWithJob(IScheduler sched, IJobDetail jobDetail)
        {
            RegisterCalendars(sched);

            if (RegisterGroups.Count > 1) // use the bulk add method if using more than 1
            {
                List<ITrigger> triggers = new List<ITrigger>();
                RegisterGroups.ForEach(x => triggers.Add(x.TriggerBuilder.Build()));

                var dic = new Dictionary<Quartz.IJobDetail, Quartz.Collection.ISet<ITrigger>>();
                Quartz.Collection.ISet<ITrigger> set = new Collection.HashSet<ITrigger>(triggers);
                dic.Add(jobDetail, set);
               
                sched.ScheduleJobs(dic, true);
            }
            else if (RegisterGroups.Count == 1)
            {
                ITrigger trigger = RegisterGroups[0].TriggerBuilder.Build();
                sched.ScheduleJob(jobDetail, trigger);
            }
        }

        #endregion
    }

    /// <summary>
    /// A Trigger and Calendar pair.
    /// </summary>
    public class RegisterGroup
    {
        public TriggerBuilder TriggerBuilder { get; private set; }
        public ICalendar Calendar { get; private set; }

        public RegisterGroup(TriggerBuilder trigger)
        {
            TriggerBuilder = trigger;
        }
        public RegisterGroup(TriggerBuilder trigger, ICalendar calendar)
        {
            TriggerBuilder = trigger;
            Calendar = calendar;
        }
    }
}
