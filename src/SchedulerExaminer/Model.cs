using Quartz;
using Quartz.Impl.Triggers;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AndrewSmith.Quartz.TextToSchedule;
using AndrewSmith.Quartz.TextToSchedule.Util;
using AndrewSmith.Quartz.TextToSchedule.Grammars;

namespace SchedulerExaminer
{
    /// <summary>
    /// The model for the MainWindow
    /// </summary>
    public class Model : DependencyObject
    {
        public const int MaxResultsToReturn = 500;

        #region Dependency Properties

        public string Input
        {
            get { return (string)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Input.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty InputProperty =
        //    DependencyProperty.Register("Input", typeof(string), typeof(Model), new PropertyMetadata(null));

        public static readonly DependencyProperty InputProperty =
            DependencyProperty.Register("Input", typeof(string), typeof(Model),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(InputChanged)));

        public string Explanation
        {
            get { return (string)GetValue(ExplanationProperty); }
            set { SetValue(ExplanationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Explanation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExplanationProperty =
            DependencyProperty.Register("Explanation", typeof(string), typeof(Model), new PropertyMetadata(null));

        public ObservableCollection<string> Triggers
        {
            get { return (ObservableCollection<string>)GetValue(TriggersProperty); }
            set { SetValue(TriggersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Triggers.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TriggersProperty =
            DependencyProperty.Register("Triggers", typeof(ObservableCollection<string>), typeof(Model), new PropertyMetadata(null));
        
        public TimeZoneInfo TimeZone
        {
            get { return (TimeZoneInfo)GetValue(TimeZoneProperty); }
            set { SetValue(TimeZoneProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimeZone.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeZoneProperty =
            DependencyProperty.Register("TimeZone", typeof(TimeZoneInfo), typeof(Model), new FrameworkPropertyMetadata(TimeZoneInfo.Local, new PropertyChangedCallback(InputChanged)));

        #endregion

        public Model()
        {
            Triggers = new ObservableCollection<string>();
        }

        private static void InputChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var m = (sender) as Model;
            m.Triggers.Clear();

            TextToScheduleResults results = null;

            TextToScheduleFactory parserFactory = new TextToScheduleFactory();
            var englishParser = parserFactory.CreateEnglishParser();
            var germanParser = parserFactory.CreateGermanParser();


            try
            {
                CronExpression cron = new CronExpression(m.Input);
                results = new TextToScheduleResults();
                results.Add(TriggerBuilder.Create()
                    .WithCronSchedule(m.Input, tb =>
                        tb.InTimeZone(m.TimeZone)));
            }
            catch
            {
                results = englishParser.Parse(m.Input, m.TimeZone);
            }

            if (results == null)
            {
                try
                {
                    results = germanParser.Parse(m.Input, m.TimeZone);
                }
                catch
                {

                }
            }
            

            if (results != null)
            {
                List<DateTimeOffset> list = new List<DateTimeOffset>();

                foreach (var groups in results.RegisterGroups)
	            {
                    var trigger = (IOperableTrigger)groups.TriggerBuilder.Build();

                    var dates = TriggerUtils.ComputeFireTimes(trigger, groups.Calendar, MaxResultsToReturn);

                    foreach (var d in dates)
                    {
                        list.Add(TimeZoneUtil.ConvertTime(d, m.TimeZone));
                    }
	            }

                //sort the list
                list.Sort();

                foreach (var item in list.Take(MaxResultsToReturn))
                {
                    m.Triggers.Add(item.ToString("ddd, MM/dd/yyyy hh:mm:ss tt (zzz)"));
                }
            }
        }
    }
}
