using LogUtils.Helpers;
using System;
using System.Threading;
using DotNetTask = System.Threading.Tasks.Task;

namespace LogUtils.Threading
{
    public class Task
    {
        /// <summary>
        /// A string used to identify a task in logging events
        /// </summary>
        public string Name = "Unknown";

        /// <summary>
        /// A value assigned to a task during initialization to distinguish it from other tasks
        /// </summary>
        public int ID;

        /// <summary>
        /// Exposes the <see cref="DotNetTask"/> used in asynchronous task operation
        /// </summary>
        internal TaskHandle Handle;

        /// <summary>
        /// Task operation invoked by this task
        /// </summary>
        protected readonly Action Run;

        private TaskProvider _runTaskAsync;
        protected TaskProvider RunAsync
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

        /// <summary>
        /// The current task completion state
        /// </summary>
        public TaskState State { get; protected set; }

        /// <summary>
        /// Checks whether an async task operation has been scheduled and has yet to be completed
        /// </summary>
        public bool IsRunning => State == TaskState.Running;

        /// <summary>
        /// Checks whether task operation runs synchronously with other tasks
        /// </summary>
        public bool IsSynchronous => RunAsync == null;

        /// <summary>
        /// Checks whether a task is in a completion state
        /// </summary>
        public bool IsCompleteOrCanceled => State == TaskState.Complete || State == TaskState.Aborted;

        /// <summary>
        /// Checks whether task operation is possible to run based on the current task state
        /// </summary>
        public bool PossibleToRun => !IsCompleteOrCanceled;

        public TimeSpan InitialTime = TimeSpan.Zero;

        public TimeSpan LastActivationTime = TimeSpan.Zero;

        public TimeSpan NextActivationTime => (HasRunOnce ? LastActivationTime : InitialTime) + WaitTimeInterval;

        /// <summary>
        /// The time to wait in between task runs
        /// </summary>
        public TimeSpan WaitTimeInterval = TimeSpan.Zero;

        /// <summary>
        /// Checks whether task has run at least once
        /// </summary>
        public bool HasRunOnce => LastActivationTime > TimeSpan.Zero;

        /// <summary>
        /// When true, task will run more than one time, instead of once
        /// </summary>
        public bool IsContinuous;

        /// <summary>
        /// Constructs a new <see cref="Task"/> object
        /// </summary>
        /// <param name="runTask">Task operation to store and execute later</param>
        /// <param name="waitTimeInMS">Timespan to wait to execute task operation when scheduled</param>
        /// <exception cref="ArgumentNullException">Task operation is null</exception>
        /// <remarks>Pass this object into <see cref="LogTasker"/> to run a task on a background thread</remarks>
        public Task(Action runTask, int waitTimeInMS)
        {
            if (runTask == null)
                throw new ArgumentNullException(nameof(runTask));

            Run = runTask;
            WaitTimeInterval = TimeSpan.FromMilliseconds(waitTimeInMS);
            Initialize();
        }

        /// <summary>
        /// Constructs a new <see cref="Task"/> object
        /// </summary>
        /// <param name="runTask">Task operation to store and execute later</param>
        /// <param name="waitTime">Timespan to wait to execute task operation when scheduled</param>
        /// <exception cref="ArgumentNullException">Task operation is null</exception>
        /// <remarks>Pass this object into <see cref="LogTasker"/> to run a task on a background thread</remarks>
        public Task(Action runTask, TimeSpan waitTime)
        {
            if (runTask == null)
                throw new ArgumentNullException(nameof(runTask));

            Run = runTask;
            WaitTimeInterval = waitTime;
            Initialize();
        }

        /// <summary>
        /// Constructs a new <see cref="Task"/> object
        /// </summary>
        /// <param name="runTaskAsync">A delegate that provides a <see cref="DotNetTask"/> operation to store and execute later</param>
        /// <param name="waitTime">Timespan to wait to execute task operation when scheduled</param>
        /// <exception cref="ArgumentNullException">Task operation is null</exception>
        /// <remarks>Pass this object into <see cref="LogTasker"/> to run a task on a background thread</remarks>
        public Task(TaskProvider runTaskAsync, TimeSpan waitTime)
        {
            if (runTaskAsync == null)
                throw new ArgumentNullException(nameof(runTaskAsync));

            RunAsync = runTaskAsync;
            Handle = new TaskHandle(this);

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
                _runTaskAsync = new TaskProvider(() =>
                {
                    Run.Invoke();
                    return DotNetTask.CompletedTask;
                });
            }
        }

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

            TaskState lastState = State;

            SetState(TaskState.Running);

            var runAsync = RunAsync;
            var handle = Handle;

            if (runAsync != null)
            {
                var task = DotNetTask.Run(new Func<DotNetTask>(runAsync));
                task.ContinueWith((task) =>
                {
                    if (PossibleToRun)
                        SetState(lastState);
                });

                if (handle != null)
                    handle.Task = task;
                return;
            }

            try
            {
                Run.Invoke();
            }
            finally
            {
                if (PossibleToRun)
                    SetState(lastState);
            }
        }

        /// <summary>
        /// Runs the task a single time before terminating
        /// </summary>
        /// <param name="force">Should this task bypass the scheduling process (in the case of an unsubmitted task)</param>
        /// <exception cref="InvalidOperationException">The task is already completed, or canceled OR the task is running on another thread, and task concurrency is not allowed</exception>
        /// <exception cref="InvalidStateException">The state has failed, or has been marked as complete</exception>
        public void RunOnceAndEnd(bool force)
        {
            IsContinuous = false;

            if (force || State != TaskState.NotSubmitted)
            {
                RunOnce();
                Complete();
                return;
            }
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
        /// <exception cref="TimeoutException">Timeout expired</exception>
        public static async DotNetTask WaitUntil(Func<bool> condition, int frequency = 5, int timeout = -1)
        {
            //Code sourced from https://stackoverflow.com/a/52357854/30273286
            var waitTask = DotNetTask.Run(async () =>
            {
                while (!condition())
                {
                    await DotNetTask.Delay(frequency).ConfigureAwait(false);
                }
            });

            using (CancellationTokenSource cancelSource = new CancellationTokenSource())
            {
                var task = DotNetTask.WhenAny(waitTask, DotNetTask.Delay(timeout, cancelSource.Token));

                await task.ConfigureAwait(false);

                if (task.Result != waitTask)
                    throw new TimeoutException();

                cancelSource.Cancel(); //Ensure that delay is canceled
            }
        }
    }

    public delegate DotNetTask TaskProvider();

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
        Running,
        Complete,
        Aborted
    }
}
