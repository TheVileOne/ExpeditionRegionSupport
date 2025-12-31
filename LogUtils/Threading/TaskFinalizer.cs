using System;
using System.Collections.Generic;
using DotNetTask = System.Threading.Tasks.Task;

namespace LogUtils.Threading
{
    /// <summary>
    /// A struct wrapper for walking through a multi-step asynchronous process
    /// </summary>
    public readonly struct TaskFinalizer
    {
        private readonly IEnumerator<DotNetTask> taskEnumerator;

        /// <summary>
        /// Gets the currently running, or completed <see cref="DotNetTask"/> object
        /// </summary>
        public readonly DotNetTask Current => taskEnumerator.Current;

        private TaskFinalizer(IEnumerator<DotNetTask> enumerator)
        {
            taskEnumerator = enumerator;          
        }

        internal void Initialize()
        {
            taskEnumerator.MoveNext(); //Ensures that task is stored in Current
        }

        /// <summary>
        /// Invoke finalizer code on an enumerated task
        /// </summary>
        /// <exception cref="InvalidOperationException">Task execution finalizer has already run</exception>
        public readonly void CompleteTask()
        {
            taskEnumerator.MoveNext();
        }

        public static TaskFinalizer CreateFinalizer(IEnumerator<DotNetTask> taskEnumerator)
        {
            return new TaskFinalizer(taskEnumerator);
        }
    }
}
