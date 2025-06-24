using LogUtils.Helpers.Comparers;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogUtils
{
    public class ExceptionInfo : IEquatable<ExceptionInfo>
    {
        public readonly Exception Exception;
        public readonly string ExceptionMessage;
        public readonly string StackTrace;

        public ExceptionInfo(Exception ex) : this(ex.Message, ex.StackTrace)
        {
            Exception = ex;
        }

        public ExceptionInfo(string message, string stackTrace)
        {
            ExceptionMessage = message ?? string.Empty;
            StackTrace = stackTrace ?? string.Empty;
        }

        public bool Equals(ExceptionInfo other)
        {
            return ExceptionComparer.DefaultComparer.Equals(this, other);
        }

        public bool Equals(ExceptionInfo other, IEqualityComparer<ExceptionInfo> comparer)
        {
            return comparer.Equals(this, other);
        }

        public override int GetHashCode()
        {
            string hashString = ExceptionMessage + StackTrace;
            return hashString.GetHashCode();
        }

        public override string ToString()
        {
            if (StackTrace == string.Empty)
                return ExceptionMessage;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(ExceptionMessage)
              .Append(StackTrace);

            const int MAX_EXCEPTIONS = 5;

            Exception exception = Exception?.InnerException;

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
