using System;

namespace LogUtils.Threading
{
    public class Task
    {
        public string Name = "Unknown";

        public int ID;

        public readonly Action Run;

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

        public bool PossibleToRun => State != TaskState.Complete && State != TaskState.Aborted;

        public TaskState State { get; protected set; }

        /// <summary>
        /// Constructs a Task object - Pass this object into LogTasker to run a task on a background thread
        /// </summary>
        public Task(Action runTask, int waitTimeInMS)
        {
            Run = runTask;
            WaitTimeInterval = TimeSpan.FromMilliseconds(waitTimeInMS);
            SetID();
            SetState(TaskState.NotSubmitted);
        }

        /// <summary>
        /// Constructs a Task object - Pass this object into LogTasker to run a task on a background thread
        /// </summary>
        public Task(Action runTask, TimeSpan waitTime)
        {
            Run = runTask;
            WaitTimeInterval = waitTime;
            SetID();
            SetState(TaskState.NotSubmitted);
        }

        /// <summary>
        /// Runs the task a single time before terminating
        /// </summary>
        /// <param name="force">Should this task bypass the scheduling process</param>
        /// <exception cref="InvalidStateException">The state has failed, or has been marked as complete</exception>
        public void RunOnceAndEnd(bool force)
        {
            IsContinuous = false;

            if (!PossibleToRun)
                throw new InvalidStateException("Unable to run");

            if (force)
            {
                Run.Invoke();
                End();
                return;
            }

            //Unforced runs should be scheduled
            if (State == TaskState.NotSubmitted)
                LogTasker.Schedule(this);
        }

        public void End()
        {
            UtilityLogger.DebugLog("Task forced to end");
            LogTasker.EndTask(this, true);
        }

        /// <summary>
        /// Sets fields back to before first activation, and task subscription, doesn't affect wait time, or run delegate
        /// </summary>
        internal void ResetToDefaults()
        {
            InitialTime = TimeSpan.Zero;
            LastActivationTime = TimeSpan.Zero;
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
