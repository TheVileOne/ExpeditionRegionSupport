using LogUtils.Helpers.Comparers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace LogUtils
{
    public class ExceptionInfo : IEquatable<ExceptionInfo>, _Exception
    {
        /// <inheritdoc cref="Exception.Message"/>
        public string Message { get; }

        /// <inheritdoc cref="Exception.StackTrace"/>
        public string StackTrace { get; }

        /// <summary>
        /// Gets the underlying <see cref="Exception"/> represented by this object. If such information is unavailable, <see langword="null"/> is returned.
        /// </summary>
        public Exception InnerException { get; }

        /// <inheritdoc cref="Exception.HelpLink"/>
        /// <exception cref="NullReferenceException">Attempted to set field without underlying <see cref="Exception"/> present</exception>
        public string HelpLink
        {
            get => InnerException?.HelpLink;
            set => InnerException.HelpLink = value; //Not supported when Exception not present
        }

        /// <inheritdoc cref="Exception.Source"/>
        /// <exception cref="NullReferenceException">Attempted to set field without underlying <see cref="Exception"/> present</exception>
        public string Source
        {
            get => InnerException?.Source;
            set => InnerException.Source = value;
        }

        /// <inheritdoc cref="Exception.TargetSite"/>
        public MethodBase TargetSite => InnerException?.TargetSite;

        public ExceptionInfo(Exception ex) : this(ex.Message, ex.StackTrace)
        {
            InnerException = ex;
        }

        public ExceptionInfo(string message, string stackTrace)
        {
            Message = message ?? string.Empty;
            StackTrace = stackTrace ?? string.Empty;
        }

        /// <inheritdoc/>
        public bool Equals(ExceptionInfo other)
        {
            return ExceptionComparer.DefaultComparer.Equals(this, other);
        }

        public bool Equals(ExceptionInfo other, IEqualityComparer<ExceptionInfo> comparer)
        {
            return comparer.Equals(this, other);
        }

        /// <inheritdoc cref="Exception.GetBaseException"/>
        public Exception GetBaseException()
        {
            return InnerException?.GetBaseException();
        }

        /// <summary>
        /// Gets the value of the <see cref="Exception.InnerException"/> property of the <see cref="Exception"/> instance represented by this object.
        /// </summary>
        public Exception GetUnderlyingInnerException()
        {
            return InnerException?.InnerException;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            string hashString = Message + StackTrace;
            return hashString.GetHashCode();
        }

        void _Exception.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            InnerException?.GetObjectData(info, context);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (StackTrace == string.Empty)
                return Message;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Message)
              .Append(StackTrace);

            const int MAX_EXCEPTIONS = 5;

            Exception exception = GetUnderlyingInnerException();

            int exceptionTotal = 1;
            while (exception != null && exceptionTotal <= MAX_EXCEPTIONS)
            {
                sb.AppendLine()
                  .AppendLine(exception.Message)
                  .Append(exception.StackTrace);
                exception = exception.InnerException;
                exceptionTotal++;
            }
            return sb.ToString();
        }
    }
}
