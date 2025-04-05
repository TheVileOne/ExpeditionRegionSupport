using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Helpers
{
    public static class StringParser
    {
        /// <summary>
        /// Splits a string by Newline characters
        /// </summary>
        public static string[] GetLines(string str, int maxEntries)
        {
            //Parse the entire string
            if (maxEntries <= 0)
                return str.Split([Environment.NewLine], StringSplitOptions.None);

            List<string> strings = new List<string>();

            int lineStart = 0, lineEnd;
            while (strings.Count < maxEntries && (lineEnd = str.IndexOf('\n', lineStart)) >= 0)
            {
                strings.Add(str.Substring(lineStart, lineEnd - lineStart).TrimEnd('\r'));
                lineStart = lineEnd + 1;
            }

            return strings.ToArray();
        }

        /// <summary>
        /// Takes an array of strings and returns a string containing the first series of characters up to the value of maxChars
        /// - or the length of the array if maxChars is less than or equal to zero. The separator string is excluded when counting characters
        /// </summary>
        public static string Format(string[] lines, string separator, int maxChars)
        {
            if (lines.Length == 0)
                return string.Empty;

            if (maxChars <= 0)
                return string.Join(separator, lines);

            int charsRemaining = maxChars;
            var linesSelector = lines.TakeWhile(s =>
            {
                if (charsRemaining <= 0)
                    return false;

                charsRemaining -= s.Length;
                return true;
            });

            string[] selectedLines = linesSelector.ToArray();

            //The value indicates that we went over the maximum allowed chars
            int charOverflow = charsRemaining < 0 ? Math.Abs(charsRemaining) : 0;

            if (charOverflow > 0)
            {
                string lastEntry = selectedLines.Last();

                //Trim the chars to match the chars we want
                selectedLines[selectedLines.Length - 1] = lastEntry.Remove(lastEntry.Length - charOverflow);
            }

            return string.Join(separator, selectedLines);
        }

        /// <summary>
        /// Trim the last trailing new line from the given string
        /// </summary>
        public static string TrimNewLine(string str)
        {
            if (str == null)
                return null;

            if (str.EndsWith("\r\n"))
                return str.Substring(0, str.Length - 2);
            
            if (str.EndsWith("\n") || str.EndsWith("\r"))
                return str.Substring(0, str.Length - 1);
            return str;
        }
    }
}
