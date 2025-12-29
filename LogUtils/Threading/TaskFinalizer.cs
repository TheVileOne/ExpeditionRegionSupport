using System;
using System.Collections;
using System.Collections.Generic;
using DotNetTask = System.Threading.Tasks.Task;

namespace LogUtils.Threading
{
    /// <summary>
    /// A struct wrapper for walking through a multi-step asynchronous process
    /// </summary>
    public readonly struct TaskFinalizer
    {
        private readonly IEnumerator taskEnumerator;

        private TaskFinalizer(IEnumerator enumerator)
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

        public static TaskFinalizer CreateFinalizer(IEnumerator taskEnumerator)
        {
            return new TaskFinalizer(taskEnumerator);
        }
    }
}
