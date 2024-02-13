using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace DD_Bot.Application.Services
{

    public class ScheduleItem
    {
        public string Name { get; }
        public string DisplayName { get; }
        public Action<List<object>> Function { get; }
        public List<object> args { get; set; }
        public ulong IntervalMS { get; set; }
        public DateTime NextExecuteTime { get; set; }

        public ScheduleItem(string name, string displayName, ulong intervalMS, Action<List<object>> func, List<object> args)
        {
            Name = name;
            DisplayName = displayName;
            args = args;
            IntervalMS = intervalMS;

            UpdateInterval(intervalMS);
        }

        public void UpdateInterval(ulong intervalMS = 0)
        {
            if (intervalMS > 0)
            {
                IntervalMS = intervalMS;
            }
            NextExecuteTime = DateTime.Now.AddMilliseconds(IntervalMS);
        }
    }

    public class SchedulerService
    {
        private static Timer clock;
        private static readonly List<ScheduleItem> scheduleItems = new List<ScheduleItem>();

        public static void Start(ulong intervalMS)
        {
            clock = new Timer
            {
                Interval = intervalMS
            };
            clock.Elapsed += new ElapsedEventHandler(ClockElapsed);
            clock.Start();
        }

        public static void Stop()
        {
            clock.Elapsed -= ClockElapsed;
            clock.Stop();
        }

        public static void AddItem(ScheduleItem item)
        {
            if (item.IntervalMS != 0)
            {
                scheduleItems.Add(item);
            }
        }

        public static void RemoveItem(string name)
        {
            int index = scheduleItems.FindIndex(item => item.Name == name);
            if (index != -1)
            {
                scheduleItems.RemoveAt(index);
            }
        }

        public static ScheduleItem GetItem(string name)
        {
            return scheduleItems.Find(item => item.Name == name);
        }

        public static IReadOnlyCollection<ScheduleItem> GetItems()
        {
            return scheduleItems.AsReadOnly();
        }

        private static void ClockElapsed(object sender, ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now;

            foreach (ScheduleItem item in scheduleItems)
            {
                if (item.IntervalMS != 0
                && now >= item.NextExecuteTime)
                {
                    try { item.Function(item.args); }
                    catch (Exception ex)
                    {
                        string exceptionMessage = $"Exception occured in ScheduleItem callback function. ScheduleItem: {item.Name}";

                        if (ex is AggregateException aggregateEx)
                        {
                            int i = 0;
                            foreach (Exception innerEx in aggregateEx.InnerExceptions)
                                Console.WriteLine($"{exceptionMessage}\n(THIS IS AN INNER EXCEPTION, NUMBER {++i})");
                        }
                        else Console.WriteLine(exceptionMessage);
                    }

                    item.UpdateInterval();
                }
            }
        }
    }
}
