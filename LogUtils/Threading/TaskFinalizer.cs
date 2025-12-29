using System;
using System.Collections;
using System.Collections.Generic;
using DotNetTask = System.Threading.Tasks.Task;

namespace LogUtils.Threading
{
    /// <summary>
    /// A struct wrapper for walking through a multi-step asynchronous process
    /// </summary>
    public readonly struct TaskFinalizer : IEnumerable<DotNetTask>
    {
        private readonly DotNetTask task;
        private readonly IEnumerator taskEnumerator;

        private TaskFinalizer(DotNetTask task)
        {
            this.task = task;

            IEnumerable enumerable = this;

            taskEnumerator = enumerable.GetEnumerator();
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

        IEnumerator<DotNetTask> IEnumerable<DotNetTask>.GetEnumerator()
        {
            yield return task; //We need to finish on the same thread we started
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return task; //We need to finish on the same thread we started
        }

        public static TaskFinalizer CreateFinalizer(DotNetTask task)
        {
            return new TaskFinalizer(task);
        }
    }
}
