using LogUtils.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace LogUtils.Threading
{
    public static class LogTasker
    {
        private static List<Task> tasksInProcess = new List<Task>();

        private static Thread _thread;
        //private static Timer _timer; //TODO: Is it worth it?

        internal static CrawlMark CurrentCrawlMark = CrawlMark.None;

        public static CrawlMark WaitOnCrawlMark = CrawlMark.None;

        /// <summary>
        /// Task thread is waiting on another thread to signal it to continue
        /// </summary>
        public static bool WaitingOnSignal { get; private set; }

        private static Queue<SyncCallback> _submissionBuffer = new Queue<SyncCallback>();

        private static object _submissionLock = new object();

        public static bool IsBatching { get; private set; }

        /// <summary>
        /// Allows tasks to be scheduled in an uninterrupted sequence
        /// </summary>
        public static void StartBatching()
        {
            lock (_submissionLock)
                IsBatching = true;
        }

        public static void SubmitBatch()
        {
            lock (_submissionLock)
            {
                IsBatching = false;

                if (_submissionBuffer.Count > 0)
                {
                    OnThreadUpdate += new SyncCallback(invokeBatch);

                    static void invokeBatch()
                    {
                        SyncCallback self = invokeBatch;
                        var submissions = _submissionBuffer;

                        while (submissions.Count > 0)
                        {
                            var submitAction = submissions.Dequeue();

                            try
                            {
                                submitAction.Invoke();
                            }
                            catch (Exception ex)
                            {
                                UtilityLogger.LogError("Task submission error", ex);
                            }
                        }
                        OnThreadUpdate -= self;
                    };
                }
            }
        }

        internal static void Start()
        {
            _thread = new Thread(threadUpdate);
            _thread.IsBackground = true;
            _thread.Start();
        }

        /// <summary>
        /// Schedules a task to run on a background thread
        /// </summary>
        /// <param name="task">Task to run</param>
        /// <exception cref="ArgumentNullException">Exception that throws when passing in a null task</exception>
        public static Task Schedule(Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (task.State != TaskState.NotSubmitted)
                throw new InvalidStateException(nameof(task));

            bool isBatched = IsBatching;
            InternalSchedule(task, addTask);

            void addTask()
            {
                SyncCallback self = addTask;
                tasksInProcess.Add(task);

                //Batched process will unsubscribe from event instead
                if (!isBatched)
                    OnThreadUpdate -= self;
            }
            return task;
        }

        /// <summary>
        /// Schedules a task to run before another task on a background thread
        /// </summary>
        /// <param name="task">Task to run</param>
        /// <param name="taskOther">Task that should run after</param>
        /// <exception cref="TaskNotFoundException">Exception that throws if the targeted task does not exist</exception>
        /// <exception cref="ArgumentNullException">Exception that throws when passing in a null task</exception>
        /// <exception cref="ArgumentException">Exception that throws if the task instances are the same</exception>
        public static Task ScheduleBefore(Task task, Task taskOther)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (task == taskOther)
                throw new ArgumentException("Tasks refer to the same instance when expecting different instances");

            if (task.State != TaskState.NotSubmitted)
                throw new InvalidStateException(nameof(task));

            bool isBatched = IsBatching;
            InternalSchedule(task, addTaskBefore);

            void addTaskBefore()
            {
                SyncCallback self = addTaskBefore;
                try
                {
                    int insertIndex = tasksInProcess.IndexOf(taskOther);

                    if (insertIndex == -1)
                    {
                        EndTask(task, true);
                        throw new TaskNotFoundException();
                    }

                    task.SetState(TaskState.Submitted);
                    tasksInProcess.Insert(insertIndex, task);
                }
                finally
                {
                    if (!isBatched)
                        OnThreadUpdate -= self;
                }
            }
            return task;
        }

        /// <summary>
        /// Schedules a task to run after another task on a background thread
        /// </summary>
        /// <param name="task">Task to run</param>
        /// <param name="taskOther">Task that should run before</param>
        /// <exception cref="TaskNotFoundException">Exception that throws if the targeted task does not exist</exception>
        /// <exception cref="ArgumentNullException">Exception that throws when passing in a null task</exception>
        /// <exception cref="ArgumentException">Exception that throws if the task instances are the same</exception>
        public static Task ScheduleAfter(Task task, Task taskOther)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (task == taskOther)
                throw new ArgumentException("Tasks refer to the same instance when expecting different instances");

            if (task.State != TaskState.NotSubmitted)
                throw new InvalidStateException(nameof(task));

            bool isBatched = IsBatching;
            InternalSchedule(task, addTaskAfter);

            void addTaskAfter()
            {
                SyncCallback self = addTaskAfter;
                try
                {
                    int insertIndex = tasksInProcess.IndexOf(taskOther);

                    if (insertIndex == -1)
                    {
                        EndTask(task, true);
                        throw new TaskNotFoundException();
                    }

                    task.SetState(TaskState.Submitted);
                    tasksInProcess.Insert(insertIndex + 1, task);
                }
                finally
                {
                    if (!isBatched)
                        OnThreadUpdate -= self;
                }
            }
            return task;
        }

        internal static void InternalSchedule(Task task, SyncCallback taskDeliveryProcess)
        {
            UtilityLogger.DebugLog("Scheduling task");
            task.SetInitialTime();

            bool isBatchedProcess = IsBatching;

            if (isBatchedProcess)
            {
                BatchTask(task, taskDeliveryProcess);
                return;
            }
            OnThreadUpdate += taskDeliveryProcess;
        }

        internal static void BatchTask(Task task, SyncCallback taskDeliveryProcess)
        {
            task.SetState(TaskState.PendingSubmission);
            _submissionBuffer.Enqueue(taskDeliveryProcess);
        }

        public static void EndTask(Task task, bool rejected)
        {
            if (task.State == TaskState.NotSubmitted) return;

            UtilityLogger.DebugLog("Task ended after " + TimeConversion.DateTimeInMilliseconds(DateTime.UtcNow - task.InitialTime) + " milliseconds");
            UtilityLogger.DebugLog("Wait interval " + task.WaitTimeInterval.TotalMilliseconds + " milliseconds");

            task.ResetToDefaults();
            task.SetState(rejected ? TaskState.Aborted : TaskState.Complete);

            tasksInProcess.Remove(task);
        }

        private static Stopwatch _timer = new Stopwatch();

        /// <summary>
        /// A debug field for detecting thread hangs
        /// </summary>
        private static int _ticksWaitedThisFrame;

        private static void threadUpdate()
        {
            Thread.CurrentThread.Name = UtilityConsts.UTILITY_NAME;

            while (true)
            {
                TimeSpan currentTime = TimeSpan.FromTicks(Stopwatch.GetTimestamp());

                _timer.Restart();
                _ticksWaitedThisFrame = 0;

                crawlMarkReached(CrawlMark.BeginUpdate);
                int tasksProcessedCount = 0;
                foreach (Task task in safeGetTasks())
                {
                    //Time since last activation, or task subscription time
                    TimeSpan timeElapsedSinceLastActivation = currentTime - (task.HasRunOnce ? task.LastActivationTime : task.InitialTime);

                    if (timeElapsedSinceLastActivation >= task.WaitTimeInterval)
                    {
                        bool taskRanWithErrors = false;
                        if (!TryRun(task))
                        {
                            task.IsContinuous = false; //Don't allow task to try again
                            taskRanWithErrors = true;
                        }

                        task.LastActivationTime = currentTime;
                        if (!task.IsContinuous)
                            EndTask(task, taskRanWithErrors);
                    }
                    tasksProcessedCount++;
                }
                crawlMarkReached(CrawlMark.EndUpdate);

                if (_timer.ElapsedMilliseconds > 5)
                    UtilityLogger.LogWarning($"Frame took longer than 5 milliseconds [{_timer.ElapsedMilliseconds} ms]");

                double waitTimeInMilliseconds = (int)(_ticksWaitedThisFrame / Stopwatch.Frequency) * 1000;

                if (waitTimeInMilliseconds > 1.0d)
                    UtilityLogger.LogWarning("Wait time took longer than 1 milliseconds");
            }
        }

        internal static bool TryRun(Task task)
        {
            try
            {
                task.Run();
                return true;
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("Task failed to execute", ex);
                return false;
            }
        }

        internal static void SyncWaitOnCrawlMark(CrawlMark crawlMark)
        {
            WaitOnCrawlMark = crawlMark;

            if (crawlMark != CrawlMark.None)
            {
                while (!WaitingOnSignal)
                    continue;
            }
        }

        public delegate void SyncCallback();
        internal static event SyncCallback OnThreadUpdate;
        internal static event SyncCallback OnThreadUpdateComplete;

        private static void crawlMarkReached(CrawlMark crawlMark)
        {
            CurrentCrawlMark = crawlMark;

            if (WaitOnCrawlMark == CurrentCrawlMark)
            {
                long ticksBeforeWait = _timer.ElapsedTicks;
                while (WaitOnCrawlMark == CurrentCrawlMark)
                {
                    WaitingOnSignal = true;
                    continue;
                }
                _ticksWaitedThisFrame += (int)(_timer.ElapsedTicks - ticksBeforeWait);
                WaitingOnSignal = false;
            }

            SyncCallback eventHandler = null;
            switch (crawlMark)
            {
                case CrawlMark.BeginUpdate:
                    eventHandler = OnThreadUpdate;
                    break;
                case CrawlMark.EndUpdate:
                    eventHandler = OnThreadUpdateComplete;
                    break;
            }

            try
            {
                eventHandler?.Invoke();
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError(ex);
            }
        }

        private static HashSet<int> handledTaskIDs = new HashSet<int>();

        private static IEnumerable<Task> safeGetTasks()
        {
            if (tasksInProcess.Count > 0)
            {
                for (int i = 0; i < tasksInProcess.Count; i++)
                {
                    Task task = tasksInProcess[i];

                    if (task != null)
                    {
                        if (!handledTaskIDs.Contains(task.ID))
                        {
                            UtilityLogger.DebugLog("Processing task: NAME " + task.Name + " ID " + task.ID);
                            UtilityLogger.DebugLog("Is Continuous " + task.IsContinuous);
                            handledTaskIDs.Add(task.ID);
                        }
                        yield return task;
                    }
                }
            }
            yield break;
        }
    }

    public enum CrawlMark
    {
        None,
        BeginUpdate,
        EndUpdate
    }
}
