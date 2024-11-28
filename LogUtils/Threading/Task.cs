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

        public TimeSpan TimeUntilNextActivation()
        {
            return NextActivationTime - new TimeSpan(DateTime.Now.Ticks);
        }
    }
}
