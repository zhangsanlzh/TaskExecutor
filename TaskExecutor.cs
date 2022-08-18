using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceCommon.TaskScheduler
{
    public class TaskExecutor
    {
        private TaskScheduler scheduler = new TaskScheduler();
        private AutoResetEvent resetEvent = new AutoResetEvent(false);

        public TaskExecutor()
        {
        }

        /// <summary>
        /// 不断执行 Action。Action 完成后接着重复执行下个 Action。除非调用了 Stop 使之停止。
        /// </summary>
        /// <param name="action"></param>
        /// <param name="usePeriod">
        /// 是否使用自定义周期。
        /// true，使用。如果不填第三个参数，将按 1s 周期性不断执行 Action。
        /// 如果传入第三个参数，将按第三个参数表示的周期不断执行 Action。
        /// false 不使用。填不填第三个参数，都不断执行 Action。
        /// </param>
        /// <param name="periodSeconds"></param>
        public void Run(Action action, bool usePeriod, double periodSeconds = 1)
        {
            var task = usePeriod ? new RecurringTask(action, TimeSpan.FromSeconds(periodSeconds)) : new RecurringTask(action);
            scheduler.TaskCompleted -= Scheduler_TaskCompleted;
            scheduler.TaskCompleted += Scheduler_TaskCompleted;
            scheduler.AddTask(task);
            scheduler.Start();
            resetEvent.WaitOne(Timeout.Infinite);
        }

        public void Run(Action action, double timeRangeSeconds)
        {
            var task = new RecurringTask(action, TimeSpan.FromSeconds(timeRangeSeconds), TimeSpan.FromSeconds(1));
            scheduler.TaskCompleted -= Scheduler_TaskCompleted;
            scheduler.TaskCompleted += Scheduler_TaskCompleted;
            scheduler.AddTask(task);
            scheduler.Start();
            resetEvent.WaitOne(Timeout.Infinite);
        }

        public void Run(Action action, double timeRangeSeconds, double periodSeconds)
        {
            var task = new RecurringTask(action, TimeSpan.FromSeconds(timeRangeSeconds), TimeSpan.FromSeconds(periodSeconds));
            scheduler.TaskCompleted -= Scheduler_TaskCompleted;
            scheduler.TaskCompleted += Scheduler_TaskCompleted;
            scheduler.AddTask(task);
            scheduler.Start();
            resetEvent.WaitOne(Timeout.Infinite);
        }

        public void Stop()
        {
            scheduler.Stop();
        }

        private void Scheduler_TaskCompleted()
        {
            resetEvent.Set();
        }
    }
}
