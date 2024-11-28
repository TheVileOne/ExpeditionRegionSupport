using System;
using System.Collections.Generic;
using System.Threading;

namespace LogUtils.Threading
{
    public static class LogTasker
    {
        private static Thread thread;
        private static List<Task> tasks = new List<Task>();

        internal static void Start()
        {
            thread = new Thread(threadUpdate);
            thread.IsBackground = true;
            thread.Start();
        }

        public static Task Schedule(Task task)
        {
            task.InitialTime = new TimeSpan(DateTime.Now.Ticks);
            tasks.Add(task);
            return task;
        }

        /// <summary>
        /// Schedules a task to run before another task
        /// </summary>
        /// <param name="task">Task to run</param>
        /// <param name="taskOther">Task that should run after</param>
        /// <exception cref="TaskNotFoundException">Exception that throws if the targeted task does not exist</exception>
        /// <exception cref="ArgumentException">Exception that throws if the task instances are the same</exception>
        public static void ScheduleBefore(Task task, Task taskOther)
        {
            int insertIndex = tasks.IndexOf(taskOther);

            if (insertIndex == -1)
                throw new TaskNotFoundException();

            if (task == taskOther)
                throw new ArgumentException("Tasks refer to the same instance when expecting different instances");

            EndTask(task); //Limit tasks list to one instance per task

            task.InitialTime = new TimeSpan(DateTime.Now.Ticks);
            tasks.Insert(insertIndex, task);
        }

        /// <summary>
        /// Schedules a task to run after another task
        /// </summary>
        /// <param name="task">Task to run</param>
        /// <param name="taskOther">Task that should run before</param>
        /// <exception cref="TaskNotFoundException">Exception that throws if the targeted task does not exist</exception>
        /// <exception cref="ArgumentException">Exception that throws if the task instances are the same</exception>
        public static void ScheduleAfter(Task task, Task taskOther)
        {
            int insertIndex = tasks.IndexOf(taskOther);

            if (insertIndex == -1)
                throw new TaskNotFoundException();

            if (task == taskOther)
                throw new ArgumentException("Tasks refer to the same instance when expecting different instances");

            EndTask(task); //Limit tasks list to one instance per task

            task.InitialTime = new TimeSpan(DateTime.Now.Ticks);
            tasks.Insert(insertIndex + 1, task);
        }

        public static void EndTask(Task task)
        {
            if (tasks.Remove(task))
                task.ResetToDefaults();
        }

        private static void threadUpdate()
        {
            Thread.CurrentThread.Name = UtilityConsts.UTILITY_NAME;

            while(true)
            {
                TimeSpan currentTime = new TimeSpan(DateTime.Now.Ticks);

                int taskIndex = 0;
                while (taskIndex < tasks.Count)
                {
                    Task task = tasks[taskIndex];

                    //Time since last activation, or task subscription time
                    TimeSpan timeElapsedSinceLastActivation = currentTime - (task.HasRunOnce ? task.LastActivationTime : task.InitialTime);

                    if (timeElapsedSinceLastActivation >= task.WaitTimeInterval)
                    {
                        task.Run();
                        task.LastActivationTime = currentTime;

                        if (!task.IsContinuous)
                        {
                            EndTask(task);
                            continue; //Next task will reuse the task index
                        }
                    }
                    taskIndex++;
                }
            }
        }
    }
}
