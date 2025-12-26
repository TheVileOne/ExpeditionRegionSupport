using System;
using System.Runtime.ExceptionServices;

namespace LogUtils.Diagnostics
{
    public class ExceptionHandler
    {
        /// <summary>
        /// Data key to access LogUtils <see cref="Exception.Data"/> for exceptions that support it
        /// </summary>
        public const string DATA_CONTEXT = "Context";

        /// <summary>
        /// The behavior that results from handling an exception
        /// </summary>
        public FailProtocol Protocol;

        /// <inheritdoc cref="OnError(Exception, object)"/>
        public virtual void OnError(Exception exception)
        {
            switch (Protocol)
            {
                //Handling the exception this way allows us to rethrow while preserving the original stacktrace
                case FailProtocol.Throw:
                    ExceptionDispatchInfo.Capture(exception).Throw();
                    break;
                case FailProtocol.LogAndIgnore:
                    LogError(exception);
                    break;
            }
        }

        /// <summary>
        /// The behavior that will happen when an <see cref="Exception"/> occurs
        /// </summary>
        /// <param name="exception">An unhandled exception</param>
        /// <param name="context">Optional contextual information to attach to an <see cref="Exception"/> object</param>
        public void OnError(Exception exception, object context)
        {
            AttachContextToException(exception, context);
            OnError(exception);
        }

        /// <summary>
        /// Logs a message when an <see cref="Exception"/> is handled
        /// </summary>
        protected virtual void LogError(Exception exception)
        {
        }

        /// <summary>
        /// Attach contextual information to an <see cref="Exception"/> object
        /// </summary>
        public static void AttachContextToException(Exception exception, object context)
        {
            exception.Data[DATA_CONTEXT] = context;
        }
    }

    /// <summary>
    /// Represents common <see cref="Exception"/> handling procedures
    /// </summary>
    public enum FailProtocol
    {
        /// <summary>An <see cref="Exception"/> will log on a move error; expect return</summary>
        LogAndIgnore,
        /// <summary>No message will log on a move error; expect a silent return</summary>
        FailSilently,
        /// <summary>No message will log on a move error; expect an exception to be thrown</summary>
        Throw,
    }
}
