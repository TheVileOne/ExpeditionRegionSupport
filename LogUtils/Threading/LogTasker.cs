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
                    }
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
                task.SetState(TaskState.Submitted);
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

        /// <summary>
        /// Sets the task state to Aborted, or Complete
        /// </summary>
        public static void EndTask(Task task, bool cancel)
        {
            if (cancel)
            {
                task.Cancel();
                return;
            }
            task.Complete();
        }

        private static void removeAfterUpdate(Task task)
        {
            OnThreadUpdateComplete += removeAfterUpdate;

            void removeAfterUpdate()
            {
                SyncCallback self = removeAfterUpdate;
                tasksInProcess.Remove(task);
                OnThreadUpdateComplete -= self;
            }
        }

        private static bool timerStarted = timerUpdate != null;
        private static Action timerUpdate; 

        public static void StartTimingFrames()
        {
            if (timerStarted) return;

            Stopwatch updateTimer = Stopwatch.StartNew();

            long framesSinceLastSlowFrame = 0,
                 elapsedTicksOnLastCheck = 0;

            timerUpdate = checkUpdateTime;

            //Check time at the start, and end of the update to allow capture the time in between updates
            OnThreadUpdate += new SyncCallback(timerUpdate);
            OnThreadUpdateComplete += new SyncCallback(timerUpdate);

            void checkUpdateTime()
            {
                const int MAX_REPORTABLE_FRAME_COUNT = 10000000;

                long elapsedTicks = updateTimer.ElapsedTicks;
                long elapsedMillisecondsThisFrame = (elapsedTicks - elapsedTicksOnLastCheck) / TimeSpan.TicksPerMillisecond;

                bool isSlowFrame = elapsedMillisecondsThisFrame > Diagnostics.Debug.LogFrameReportThreshold;

                if (isSlowFrame)
                {
                    bool maxFrameCountReached = framesSinceLastSlowFrame >= MAX_REPORTABLE_FRAME_COUNT;

                    string frameCountReport = !maxFrameCountReached ? framesSinceLastSlowFrame.ToString() : $"Over {MAX_REPORTABLE_FRAME_COUNT} frames";

                    UtilityLogger.Logger.LogDebug($"Frames since last report: {frameCountReport}");
                    UtilityLogger.Logger.LogDebug($"Frame took longer than {Diagnostics.Debug.LogFrameReportThreshold} milliseconds [{elapsedMillisecondsThisFrame} ms]");
                    framesSinceLastSlowFrame = 0;
                }
                else if (CurrentCrawlMark == CrawlMark.BeginUpdate && framesSinceLastSlowFrame < MAX_REPORTABLE_FRAME_COUNT)
                    framesSinceLastSlowFrame++;

                elapsedTicksOnLastCheck = elapsedTicks;
            }
        }

        public static void StopTimingFrames()
        {
            if (!timerStarted) return;

            OnThreadUpdate -= new SyncCallback(timerUpdate);
            OnThreadUpdateComplete -= new SyncCallback(timerUpdate);
            timerUpdate = null;
        }

        private static void threadUpdate()
        {
            Thread.CurrentThread.Name = UtilityConsts.UTILITY_NAME;

            if (UtilityCore.Build == UtilitySetup.Build.DEVELOPMENT)
                StartTimingFrames();

            HashSet<int> handledTaskIDs = new HashSet<int>();

            while (true)
            {
                TimeSpan currentTime = TimeSpan.FromTicks(DateTime.UtcNow.Ticks);

                crawlMarkReached(CrawlMark.BeginUpdate);

                for (int i = 0; i < tasksInProcess.Count; i++)
                {
                    Task task = tasksInProcess[i];

                    if (handledTaskIDs.Add(task.ID))
                    {
                        UtilityLogger.DebugLog("Processing task: NAME " + task.Name + " ID " + task.ID);
                        UtilityLogger.DebugLog("Is Continuous " + task.IsContinuous);
                    }

                    if (!task.PossibleToRun)
                    {
                        removeAfterUpdate(task);
                        continue;
                    }

                    //Time since last activation, or task subscription time
                    TimeSpan timeElapsedSinceLastActivation = currentTime - (task.HasRunOnce ? task.LastActivationTime : task.InitialTime);

                    if (timeElapsedSinceLastActivation >= task.WaitTimeInterval)
                    {
                        TaskResult result = task.TryRun();

                        if (result == TaskResult.AlreadyRunning)
                            continue;

                        if (result != TaskResult.Success)
                            task.IsContinuous = false; //Don't allow task to try again

                        task.LastActivationTime = currentTime;
                        if (!task.IsContinuous)
                        {
                            EndTask(task, result == TaskResult.Error);
                            removeAfterUpdate(task);
                        }
                    }
                }
                crawlMarkReached(CrawlMark.EndUpdate);
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
                while (WaitOnCrawlMark == CurrentCrawlMark)
                {
                    WaitingOnSignal = true;
                    continue;
                }
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
    }

    public enum CrawlMark
    {
        None,
        BeginUpdate,
        EndUpdate
    }
}
