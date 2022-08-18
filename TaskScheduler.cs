using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceCommon.TaskScheduler
{
    internal class TaskScheduler : IDisposable
    {
        private TaskCollection taskQueue;

        private Thread thread;

        private bool started = false;

        public TaskScheduler()
        {
            taskQueue = new TaskCollection();
        }

        public event Action TaskCompleted;

        public bool Started
        {
            get { return started; }
        }

        public int TaskCount
        {
            get { return taskQueue.Count; }
        }

        /// <summary>
        /// Start running tasks
        /// </summary>
        public void Start()
        {
            lock (taskQueue)
            {
                if (!started)
                {
                    started = true;
                    thread = new Thread(Run);
                    thread.Start();
                }
            }
        }

        public void Stop()
        {
            Debug.WriteLine("Task Schedular thread stopping", "Stop");
            started = false;
            TaskCompleted?.Invoke();
            thread.Join();
            Debug.WriteLine("Task Schedular thread stopped", "Stop");
        }

        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task">Once ITask object is added, it should never be updated from outside TaskScheduler</param>
        public void AddTask(ITask task)
        {
            ITask earliestTask;

            lock (taskQueue)
            {
                earliestTask = GetEarliestScheduledTask();
                taskQueue.Add(task);
            }
        }

        private void Run()
        {
            Debug.WriteLine("Task Schedular thread starting", "Run");

            while (started)
            {
                try
                {
                    ITask task = GetEarliestScheduledTask();
                    if (task != null)
                    {
                        Debug.WriteLine("Starting task ", "Run");
                        try
                        {
                            task.Run();
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Exception while running Task", "Run");
                            Debug.WriteLine(e, "Run");
                            LogHelp.Error(e.ToString());
                        }

                        lock (taskQueue)
                        {
                            taskQueue.Remove(task);
                        }
                        ReScheduleRecurringTask(task);
                    }
                    else
                    {
                        Stop();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e, "Run");
                    LogHelp.Error(e.ToString());
                }
            }
        }

        private void ReScheduleRecurringTask(ITask task)
        {
            if (DateTime.Now - task.StartTime >= task.TimeRange)
            {
                Stop();
                return;
            }

            DateTime nextRunTime = task.GetNextRunTime(task.StartTime);
            if (nextRunTime != DateTime.MinValue)
            {
                lock (taskQueue)
                {
                    taskQueue.Add(task);
                }
                Debug.WriteLine("Recurring task scheduled for " + task.StartTime.ToString(), "ReScheduleRecurringTask");
            }
        }

        private ITask GetEarliestScheduledTask()
        {
            lock (taskQueue)
            {
                return taskQueue.First();
            }
        }
    }
}
