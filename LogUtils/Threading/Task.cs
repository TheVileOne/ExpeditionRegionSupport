using LogUtils.Helpers;
using System;
using DotNetTask = System.Threading.Tasks.Task;

namespace LogUtils.Threading
{
    public class Task
    {
        public string Name = "Unknown";

        public int ID;

        public bool IsRunning
        {
            get
            {
                var task = Handle?.Task;
                return task != null && task.Status == System.Threading.Tasks.TaskStatus.Running;
            }
        }

        public bool IsSynchronous => RunAsync == null;

        /// <summary>
        /// Useful for awaiting on this task asynchronously
        /// </summary>
        internal TaskHandle Handle;

        protected readonly Action Run;

        private AsyncTaskDelegate _runTaskAsync;
        protected AsyncTaskDelegate RunAsync
        {
            get
            {
                updateAsyncState();
                return _runTaskAsync;
            }
            private set
            {
                _runTaskAsync = value;
            }
        }

        public TimeSpan InitialTime = TimeSpan.Zero;

        public TimeSpan LastActivationTime = TimeSpan.Zero;

        public TimeSpan NextActivationTime => (HasRunOnce ? LastActivationTime : InitialTime) + WaitTimeInterval;

        /// <summary>
        /// The time to wait in between task runs
        /// </summary>
        public TimeSpan WaitTimeInterval = TimeSpan.Zero;

        /// <summary>
        /// A flag that indicates whether task has run at least once
        /// </summary>
        public bool HasRunOnce => LastActivationTime > TimeSpan.Zero;

        /// <summary>
        /// When true, task will run more than one time, instead of once
        /// </summary>
        public bool IsContinuous;

        public bool PossibleToRun => !IsCompleteOrCanceled;

        public TaskState State { get; protected set; }

        /// <summary>
        /// Constructs a Task object - Pass this object into LogTasker to run a task on a background thread
        /// </summary>
        public Task(Action runTask, int waitTimeInMS)
        {
            if (runTask == null)
                throw new ArgumentNullException(nameof(runTask));

            Run = runTask;
            WaitTimeInterval = TimeSpan.FromMilliseconds(waitTimeInMS);
            Initialize();
        }

        /// <summary>
        /// Constructs a Task object - Pass this object into LogTasker to run a task on a background thread
        /// </summary>
        public Task(Action runTask, TimeSpan waitTime)
        {
            if (runTask == null)
                throw new ArgumentNullException(nameof(runTask));

            Run = runTask;
            WaitTimeInterval = waitTime;
            Initialize();
        }

        /// <summary>
        /// Constructs a Task object - Pass this object into LogTasker to run a task on a background thread
        /// </summary>
        public Task(AsyncTaskDelegate runTaskAsync, TimeSpan waitTime)
        {
            if (runTaskAsync == null)
                throw new ArgumentNullException(nameof(runTaskAsync));

            RunAsync = runTaskAsync;
            WaitTimeInterval = waitTime;
            Initialize();
        }

        internal void Initialize()
        {
            SetID();
            SetState(TaskState.NotSubmitted);
        }

        /// <summary>
        /// Get an awaitable handle that will complete when the task ends
        /// </summary>
        public TaskHandle GetAsyncHandle()
        {
            if (Handle == null)
                Handle = new TaskHandle(this);

            Handle.OnAccess();
            return Handle;
        }

        private void updateAsyncState()
        {
            var runTask = _runTaskAsync;

            //TODO: This is not thread-safe
            if (runTask != null)
            {
                //Handle was disposed - we no longer need to run asynchronously
                if (Handle == null && Run != null)
                    _runTaskAsync = null;
                return;
            }

            //Lazily wrap the current run task with an async wrapper if an async one is unavailable
            if (Handle != null)
            {
                _runTaskAsync = new AsyncTaskDelegate(() =>
                {
                    Run.Invoke();
                    return DotNetTask.CompletedTask;
                });
            }
        }

        public bool IsCompleteOrCanceled => State == TaskState.Complete || State == TaskState.Aborted;

        public TaskResult TryRun()
        {
            if (!PossibleToRun)
                return TaskResult.UnableToRun;

            if (IsRunning)
                return TaskResult.AlreadyRunning;

            try
            {
                RunOnce();
                return TaskResult.Success;
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("Task failed to execute", ex);
                return TaskResult.Error;
            }
        }

        /// <summary>
        /// Runs the task a single time
        /// </summary>
        /// <exception cref="InvalidOperationException">The task is already completed, or canceled OR the task is running on another thread, and task concurrency is not allowed</exception>
        public void RunOnce()
        {
            if (!PossibleToRun)
                throw new InvalidOperationException("Unable to run");

            //TODO: Allow opt-in for concurrent run state
            if (IsRunning)
                throw new InvalidOperationException("Task is not allowed to run concurrently");

            var runAsync = RunAsync;
            var handle = Handle;

            if (runAsync != null)
            {
                var task = DotNetTask.Run(new Func<DotNetTask>(runAsync));

                if (handle != null)
                    handle.Task = task;
                return;
            }
            Run.Invoke();
        }

        /// <summary>
        /// Runs the task a single time before terminating
        /// </summary>
        /// <param name="force">Should this task bypass the scheduling process</param>
        /// <exception cref="InvalidOperationException">The task is already completed, or canceled OR the task is running on another thread, and task concurrency is not allowed</exception>
        /// <exception cref="InvalidStateException">The state has failed, or has been marked as complete</exception>
        public void RunOnceAndEnd(bool force)
        {
            IsContinuous = false;

            if (force)
            {
                RunOnce();
                Complete();
                return;
            }

            //Unforced runs should be scheduled
            if (State == TaskState.NotSubmitted)
                LogTasker.Schedule(this);
        }

        /// <summary>
        /// Task will no longer run - asynchronous operations that support cancel operations will be notified
        /// </summary>
        public void Cancel()
        {
            if (State == TaskState.Complete)
            {
                UtilityLogger.LogWarning("Failed to cancel - task already completed");
                return;
            }

            if (State == TaskState.Aborted) return;

            End(TaskState.Aborted);

            var handle = Handle;

            if (handle?.IsValid == true)
                handle.CancellationToken.Cancel();
        }

        /// <summary>
        /// Task will no longer run - asynchronous operations started by the task may still continue
        /// </summary>
        public void Complete()
        {
            if (State == TaskState.Aborted)
            {
                UtilityLogger.LogWarning("Failed to complete - task was canceled");
                return;
            }
            End(TaskState.Complete);
        }

        protected void End(TaskState endState)
        {
            if (State == TaskState.NotSubmitted || !PossibleToRun) return;

            UtilityLogger.DebugLog("Task ended after " + TimeConversion.ToMilliseconds(DateTime.UtcNow - InitialTime) + " milliseconds");
            UtilityLogger.DebugLog("Wait interval " + WaitTimeInterval.TotalMilliseconds + " milliseconds");

            SetState(endState);
        }

        /// <summary>
        /// Positions this task before another task in the task run list
        /// </summary>
        /// <param name="otherTask">The task that shall run after</param>
        /// <exception cref="TaskNotFoundException">Exception that throws if the targeted task does not exist</exception>
        /// <exception cref="ArgumentException">Exception that throws if the task instances are the same</exception>
        public void RunBefore(Task otherTask)
        {
            LogTasker.ScheduleBefore(this, otherTask);
        }

        /// <summary>
        /// Positions this task after another task in the task run list
        /// </summary>
        /// <param name="otherTask">The task that shall run before</param>
        /// <exception cref="TaskNotFoundException">Exception that throws if the targeted task does not exist</exception>
        /// <exception cref="ArgumentException">Exception that throws if the task instances are the same</exception>
        public void RunAfter(Task otherTask)
        {
            LogTasker.ScheduleAfter(this, otherTask);
        }

        public void SetID()
        {
            ID = UnityEngine.Random.Range(0, 1000);
        }

        public void SetInitialTime()
        {
            InitialTime = new TimeSpan(DateTime.UtcNow.Ticks);
        }

        public void SetState(TaskState state)
        {
            State = state;
        }

        public TimeSpan TimeUntilNextActivation()
        {
            return NextActivationTime - new TimeSpan(DateTime.UtcNow.Ticks);
        }

        /// <summary>
        /// Blocks until condition is true or timeout occurs.
        /// </summary>
        /// <param name="condition">The break condition</param>
        /// <param name="frequency">The frequency at which the condition will be checked</param>
        /// <param name="timeout">The timeout in milliseconds</param>
        public static async DotNetTask WaitUntil(Func<bool> condition, int frequency = 5, int timeout = -1)
        {
            //Code sourced from https://stackoverflow.com/a/52357854/30273286
            var waitTask = DotNetTask.Run(async () =>
            {
                while (!condition()) await DotNetTask.Delay(frequency);
            });

            bool taskRun = waitTask.Wait(timeout);

            if (!taskRun)
                throw new TimeoutException();

            /*
            if (waitTask != await DotNetTask.WhenAny(waitTask, DotNetTask.Delay(timeout)))
            {
                throw new TimeoutException();
            }
            */
        }
    }

    public delegate DotNetTask AsyncTaskDelegate();

    public enum TaskResult
    {
        UnableToRun,
        AlreadyRunning,
        Error,
        Success
    }

    public enum TaskState
    {
        NotSubmitted,
        PendingSubmission,
        Submitted,
        Complete,
        Aborted
    }
}
