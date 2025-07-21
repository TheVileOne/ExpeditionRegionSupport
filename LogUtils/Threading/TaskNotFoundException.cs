using System;

namespace LogUtils.Threading
{
    /// <summary>
    /// An exception that is thrown when a Task is expected to be found, but the Task could not be located
    /// </summary>
    public class TaskNotFoundException : InvalidOperationException
    {
        public TaskNotFoundException() : base() { }

        public TaskNotFoundException(string message) : base(message)
        {
        }
    }
}
