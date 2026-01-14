using System;
using System.Runtime.Serialization;

namespace LogUtils.Threading
{
    /// <summary>
    /// Exception is thrown when an object is in an invalid state for the current operation
    /// </summary>
    public class InvalidStateException : InvalidOperationException
    {
        /// <summary>
        /// Message that will be used by default if one is not specified
        /// </summary>
        public const string DEFAULT_MESSAGE = "Task is not in a valid state.";

        /// <summary>
        /// Message that will be used by default if one is not specified, and <see cref="Task"/> is provided
        /// </summary>
        public const string DEFAULT_MESSAGE_FORMAT = DEFAULT_MESSAGE + " NAME {0} ID {1}";

        /// <summary/>
        public InvalidStateException() : base(DEFAULT_MESSAGE) { }

        /// <summary/>
        public InvalidStateException(Task task) : base(string.Format(DEFAULT_MESSAGE_FORMAT, task.Name, task.ID)) { }

        /// <summary/>
        public InvalidStateException(string message) : base(message) { }

        /// <summary/>
        public InvalidStateException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary/>
        protected InvalidStateException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
