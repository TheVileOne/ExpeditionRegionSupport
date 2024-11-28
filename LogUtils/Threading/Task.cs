using System;

namespace LogUtils.Threading
{
    public class Task
    {
        public readonly Action Run;

        /// <summary>
        /// The schedule time for the task
        /// </summary>
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

        /// <summary>
        /// Constructs a Task object - Pass this object into LogTasker to run a task on a background thread
        /// </summary>
        public Task(Action runTask, int waitTimeInMS)
        {
            Run = runTask;
            WaitTimeInterval = TimeSpan.FromMilliseconds(waitTimeInMS);
        }

        /// <summary>
        /// Constructs a Task object - Pass this object into LogTasker to run a task on a background thread
        /// </summary>
        public Task(Action runTask, TimeSpan waitTime)
        {
            Run = runTask;
            WaitTimeInterval = waitTime;
        }

        public void End()
        {
            LogTasker.EndTask(this);
        }

        /// <summary>
        /// Sets fields back to before first activation, and task subscription, doesn't affect wait time, or run delegate
        /// </summary>
        internal void ResetToDefaults()
        {
            InitialTime = TimeSpan.Zero;
            LastActivationTime = TimeSpan.Zero;
        }

        public TimeSpan TimeUntilNextActivation()
        {
            return NextActivationTime - new TimeSpan(DateTime.Now.Ticks);
        }
    }
}
