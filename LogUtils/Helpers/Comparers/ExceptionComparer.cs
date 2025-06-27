using LogUtils.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogUtils.Helpers.Comparers
{
    public class ExceptionComparer : IEqualityComparer<ExceptionInfo>
    {
        public static IEqualityComparer<ExceptionInfo> DefaultComparer = new ExceptionComparer(MAX_LINES_DEFAULT, MAX_CHARS_DEFAULT);

        /// <summary>
        /// The maximum amount of characters to check for exception equality
        /// </summary>
        public const uint MAX_CHARS_DEFAULT = 1500;

        /// <summary>
        /// The maximum amount of new lines to check for exception equality
        /// </summary>
        public const uint MAX_LINES_DEFAULT = 6;

        /// <summary>
        /// The maximum amount of new lines that can be used to evaluate equality - when set to zero, all lines will be checked
        /// </summary>
        public uint MaxLinesToCheck = 0;

        /// <summary>
        /// The maximum amount of characters that can be used to evaluate equality - when set to zero, no limit will be applied
        /// </summary>
        public uint MaxCharsToCheck = 0;

        /// <summary>
        /// Creates an ExceptionComparer instance
        /// </summary>
        public ExceptionComparer(uint maxLinesToCheck, uint maxCharsToCheck)
        {
            MaxLinesToCheck = maxLinesToCheck;
            MaxCharsToCheck = maxCharsToCheck;
        }

        public bool Equals(ExceptionInfo exception, ExceptionInfo exceptionOther)
        {
            if (exception == null)
                return exceptionOther == null;

            if (exceptionOther == null)
                return false;

            if (MaxLinesToCheck > 0)
                return checkMatchLineByLine(exception, exceptionOther);
            return checkMatch(exception, exceptionOther);
        }

        private bool checkMatch(ExceptionInfo exception, ExceptionInfo exceptionOther)
        {
            uint maxChars = MaxCharsToCheck;

            return checkMatch(exception.ExceptionMessage, exceptionOther.ExceptionMessage) && checkMatch(exception.StackTrace, exceptionOther.StackTrace);

            bool checkMatch(string str, string str2)
            {
                if (maxChars <= 0 || (str.Length <= maxChars && str2.Length <= maxChars))
                    return string.Equals(str, str2);

                //We know that both strings are greater than the threshold
                if (str.Length <= str2.Length)
                    return str.StartsWith(str2.Substring(0, (int)maxChars));

                return str2.StartsWith(str.Substring(0, (int)maxChars));
            }
        }

        private bool checkMatchLineByLine(ExceptionInfo exception, ExceptionInfo exceptionOther)
        {
            int maxLines = (int)MaxLinesToCheck;

            return checkMatch(exception.ExceptionMessage, exceptionOther.ExceptionMessage) && checkMatch(exception.StackTrace, exceptionOther.StackTrace);

            bool checkMatch(string str, string str2)
            {
                //These two arrays are guaranteed to be the same length when line count is limited, but not when 
                string[] lines = StringParser.GetLines(str, maxLines);
                string[] linesOther = StringParser.GetLines(str2, maxLines > 0 ? Math.Max(lines.Length, 1) : 0);

                //Ensure that the shortest array is always stored in lines
                if (linesOther.Length < lines.Length)
                {
                    var swapValue = lines;
                    lines = linesOther;
                    linesOther = swapValue;
                }

                bool matchFound = false;

                int charsRemaining = MaxCharsToCheck > 0 ? (int)MaxCharsToCheck : int.MaxValue;
                for (int i = 0; i < lines.Length; i++)
                {
                    str = lines[i];
                    str2 = linesOther[i];

                    //Ensure that the shortest string is always stored in str
                    if (str2.Length < str.Length)
                    {
                        var swapValue = str;
                        str = str2;
                        str2 = swapValue;
                    }

                    //We know that str can fit inside str2, but we may not have enough chars left to check the entire string
                    if (charsRemaining < str.Length)
                        str = str.Substring(0, charsRemaining);

                    matchFound = str2.StartsWith(str);
                    charsRemaining -= str.Length;

                    if (!matchFound || charsRemaining <= 0)
                        break;
                }

                //After we run out of data to match, the remaining data will be unmatched
                if (lines.Length != linesOther.Length && charsRemaining > 0)
                    matchFound = false;

                return matchFound;
            }
        }

        public int GetHashCode(ExceptionInfo obj)
        {
            if (obj == null)
                return 0;

            if (MaxLinesToCheck <= 0 && MaxCharsToCheck <= 0)
                return obj.GetHashCode();

            string hashString = string.Empty;
            int maxChars = (int)MaxCharsToCheck;
            if (MaxLinesToCheck > 0)
            {
                string[] lines;
                int maxLines = (int)MaxLinesToCheck;

                lines = StringParser.GetLines(obj.ExceptionMessage, maxLines);
                hashString += StringParser.Format(lines, Environment.NewLine, maxChars);

                lines = StringParser.GetLines(obj.StackTrace, maxLines);
                hashString += StringParser.Format(lines, Environment.NewLine, maxChars);
            }
            else //MaxCharsToCheck must be greater than zero here
            {
                hashString += (obj.ExceptionMessage.Length <= maxChars) ? obj.ExceptionMessage : obj.ExceptionMessage.Substring(0, maxChars);
                hashString += (obj.StackTrace.Length <= maxChars) ? obj.StackTrace : obj.StackTrace.Substring(0, maxChars);
            }
            return hashString.GetHashCode();
        }
    }
}
