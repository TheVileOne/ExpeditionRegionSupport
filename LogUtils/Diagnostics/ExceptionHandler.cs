using System;

namespace LogUtils.Diagnostics
{
    public abstract class ExceptionHandler
    {
        /// <summary>
        /// Data key to access LogUtils <see cref="Exception.Data"/> for exceptions that support it
        /// </summary>
        public const string DATA_CONTEXT = "Context";

        /// <inheritdoc cref="OnError(Exception, object)"/>
        public abstract void OnError(Exception exception);

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
        /// Attach contextual information to an <see cref="Exception"/> object
        /// </summary>
        public static void AttachContextToException(Exception exception, object context)
        {
            exception.Data[DATA_CONTEXT] = context;
        }
    }
}
