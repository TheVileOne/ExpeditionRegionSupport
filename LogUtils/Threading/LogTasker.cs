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

        public static void EndTask(Task task)
        {
            task.ResetToDefaults();
            tasks.Remove(task);
        }

        private static void threadUpdate()
        {
            Thread.CurrentThread.Name = "LogUtils";

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
