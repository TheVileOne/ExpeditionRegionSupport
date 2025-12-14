using System;
using static LogUtils.Threading.Lock;

namespace LogUtils.Threading
{
    /// <summary>
    /// An <see cref="Exception"/> type that is thrown when an exception is captured from a lock monitoring event
    /// </summary>
    public class LockInvocationException : InvalidOperationException
    {
        public Lock Target { get; }
        public EventID Context { get; }

        public LockInvocationException(Lock target, EventID context) : base(getExceptionMessage(context))
        {
            Target = target;
            Context = context;
        }

        public LockInvocationException(Lock target, EventID context, string message) : base(message ?? getExceptionMessage(context))
        {
            Target = target;
            Context = context;
        }

        public LockInvocationException(Lock target, EventID context, Exception ex) : base(getExceptionMessage(context), ex)
        {
            Target = target;
            Context = context;
        }

        private static string getExceptionMessage(EventID context)
        {
            return context == EventID.WaitingToAcquire
                ? "Lock monitoring was aborted"
                : "Lock monitoring was interrupted by an event invocation";
        }
    }
}
