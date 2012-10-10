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

namespace SchedulerExaminer
{
    /// <summary>
    /// The model for the MainWindow
    /// </summary>
    public class Model : DependencyObject
    {
        public const int MaxResultsToReturn = 200;

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
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(InputChanged)));

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


        public Model()
        {
            Triggers = new ObservableCollection<string>();
        }

        private static void InputChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var m = (sender) as Model;
            m.Triggers.Clear();

            TextToScheduleResults results = null;

            try
            {
                CronExpression cron = new CronExpression(m.Input);
                results = new TextToScheduleResults();
                results.Add(TriggerBuilder.Create()
                    .WithCronSchedule(m.Input));
            }
            catch
            {
                var english = TextToScheduleFactory.CreateEnglishParser();
                results = english.Parse(m.Input);
            }

            if (results == null)
            {
                try
                {
                    //try german verion
                    var german = new TextToSchedule(new AndrewSmith.Quartz.TextToSchedule.Grammars.GermanGrammar(), new AndrewSmith.Quartz.TextToSchedule.Grammars.GermanGrammarHelper());
                    results = german.Parse(m.Input);
                }
                catch
                {

                }
            }
            

            if (results != null)
            {            
                List<DateTime> list = new List<DateTime>();

                foreach (var groups in results.RegisterGroups)
	            {
                    var trigger = (IOperableTrigger)groups.TriggerBuilder.Build();
                    var dates = TriggerUtils.ComputeFireTimes(trigger, groups.Calendar, MaxResultsToReturn);

                    foreach (var d in dates)
                    {
                        list.Add(d.ToLocalTime().DateTime);
                    }
	            }

                //sort the list
                list.Sort();

                foreach (var item in list.Take(MaxResultsToReturn))
                {
                    m.Triggers.Add(item.ToString("ddd, MM/dd/yyyy hh:mm:ss tt"));
                }
            }
        }
    }
}
