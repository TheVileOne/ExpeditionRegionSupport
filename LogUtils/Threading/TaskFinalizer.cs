using LogUtils.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using DotNetTask = System.Threading.Tasks.Task;

namespace LogUtils.Threading
{
    /// <summary>
    /// A struct wrapper for walking through a multi-step asynchronous process
    /// </summary>
    public readonly struct TaskFinalizer
    {
        private readonly CancellationTokenSource tokenSource;
        private readonly IEnumerator<DotNetTask> taskEnumerator;

        /// <summary>
        /// Gets the currently running, or completed <see cref="DotNetTask"/> object
        /// </summary>
        public readonly DotNetTask Current => taskEnumerator.Current;

        /// <summary>
        /// Retrieve a token for handling task cancellation events
        /// </summary>
        public CancellationToken CancellationToken => tokenSource.Token;

        private TaskFinalizer(IEnumerator<DotNetTask> enumerator)
        {
            tokenSource = new CancellationTokenSource();
            taskEnumerator = enumerator;
        }

        internal void Initialize()
        {
            Assert.That(taskEnumerator.Current, AssertBehavior.Throw).IsNull();
            taskEnumerator.MoveNext(); //Ensures that task is stored in Current
        }

        /// <summary>
        /// Invokes cancellation event before invoking finalizer code on the current task
        /// </summary>
        public readonly void Cancel()
        {
            tokenSource.Cancel();
            EndCurrent();
        }

        /// <summary>
        /// Invoke finalizer code on the current task
        /// </summary>
        public readonly void Complete()
        {
            EndCurrent();
        }

        internal readonly void EndCurrent()
        {
            if (taskEnumerator.Current == null)
            {
                UtilityLogger.Log("Task already completed");
                return;
            }
            taskEnumerator.MoveNext();
        }

        public static TaskFinalizer CreateFinalizer(IEnumerator<DotNetTask> taskEnumerator)
        {
            return new TaskFinalizer(taskEnumerator);
        }
    }
}
