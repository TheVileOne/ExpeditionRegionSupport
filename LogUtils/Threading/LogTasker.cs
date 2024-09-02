using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogUtils.Threading
{
    public static class LogTasker
    {
        public static Task ActiveTask;

        public static Queue<Task> AwaitingTasks = new Queue<Task>();

        public static Thread TaskThread;

        /// <summary>
        /// The ThreadID currently processing logging tasks, -1 if no tasks are running
        /// </summary>
        public static int TaskThreadID => TaskThread != null ? TaskThread.ManagedThreadId : -1;

        public static bool RunningTasksOnCurrentThread => Thread.CurrentThread.ManagedThreadId == TaskThreadID;

        public static void RunTask(Action work)
        {
            Task task = new Task(work);

            if (ActiveTask != null && !ActiveTask.IsCompleted)
            {
                AwaitingTasks.Enqueue(task);
            }
            else
            {
                ActiveTask = task;

                ActiveTask.Start();
                ActiveTask.ContinueWith(taskAfter);

                static void taskAfter(Task t)
                {
                    t.Dispose();

                    if (AwaitingTasks.Any())
                    {
                        ActiveTask = AwaitingTasks.Dequeue();
                        ActiveTask.Start();
                        ActiveTask.ContinueWith(taskAfter);
                    }
                    else
                    {
                        ActiveTask = null;
                    }
                }
            }
        }
    }
}
