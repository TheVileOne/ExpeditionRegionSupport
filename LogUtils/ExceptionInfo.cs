using System;
using System.Text;

namespace LogUtils
{
    public class ExceptionInfo : IEquatable<ExceptionInfo>
    {
        /// <summary>
        /// The maximum amount of characters to check for exception equality
        /// </summary>
        public static int CompareThreshold = 1500;

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
            return checkMatch(ExceptionMessage, other.ExceptionMessage) && checkMatch(StackTrace, other.StackTrace);

            static bool checkMatch(string str, string str2)
            {
                if (str.Length <= CompareThreshold && str2.Length <= CompareThreshold)
                    return string.Equals(str, str2);

                //We know that both strings are greater than the threshold
                if (str.Length == str2.Length || str2.Length > str.Length)
                    return str.StartsWith(str2.Substring(0, CompareThreshold));

                return str2.StartsWith(str.Substring(0, CompareThreshold));
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (StackTrace != string.Empty)
            {
                sb.AppendLine(ExceptionMessage)
                  .Append(StackTrace);

                if (Exception?.InnerException != null)
                {
                    sb.AppendLine()
                      .AppendLine(Exception.InnerException.Message)
                      .Append(Exception.InnerException.StackTrace);
                }
            }
            else
            {
                sb.Append(ExceptionMessage);
            }
            return sb.ToString();
        }
    }
}
