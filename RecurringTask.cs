using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceCommon.TaskScheduler
{
    internal class RecurringTask : ITask
    {
        public RecurringTask(Action taskAction)
        {
            TaskAction = taskAction;
            StartTime = DateTime.Now;
            TimeRange = TimeSpan.MaxValue;
            Recurrence = TimeSpan.FromMilliseconds(1);
        }

        public RecurringTask(Action taskAction, TimeSpan recurrence)
        {
            TaskAction = taskAction;
            StartTime = DateTime.Now;
            TimeRange = TimeSpan.MaxValue;
            Recurrence = recurrence;
        }

        public RecurringTask(Action taskAction, TimeSpan timeRange, TimeSpan recurrence)
        {
            TaskAction = taskAction;
            TimeRange = timeRange;
            StartTime = DateTime.Now;
            Recurrence = recurrence;
        }

        public DateTime StartTime { get; set; }

        public Action TaskAction { get; set; }

        /// <summary>
        /// TimeSpan.Zero mean null
        /// </summary>
        public TimeSpan Recurrence { get; set; }

        public TimeSpan TimeRange { get; set; }

        public void Run()
        {
            Thread.Sleep(Recurrence);
            TaskAction();
        }

        public DateTime GetNextRunTime(DateTime lastExecutionTime)
        {
            if (Recurrence != TimeSpan.Zero)
            {
                return lastExecutionTime.Add(Recurrence);
            }
            else
            {
                return DateTime.MinValue;
            }
        }
    }
}
