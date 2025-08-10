using UnityEngine;

namespace LogUtils.Console
{
    /// <summary>
    /// This class converts color information into an ANSI color code
    /// </summary>
    public static class AnsiColorConverter
    {
        /// <summary>
        /// The presence of this char indicates an ANSI color code has terminated
        /// </summary>
        public const char ANSI_TERMINATOR_CHAR = 'm';

        /// <summary>
        /// Escape character for an ANSI color code
        /// </summary>
        public const char ANSI_ESCAPE_CHAR = '\x1b';

        /// <summary>
        /// Surrounds a message with ANSI codes necessary to display the message in the console with a specified color, and reset back to the default color at the end of the message
        /// </summary>
        public static string ApplyFormat(string message, Color messageColor)
        {
            //Convert Unity color data to an ANSI escape code
            string ansiForeground = AnsiToForeground(messageColor);

            //Build the message string with ANSI code prepended and a reset at the end
            return string.Concat(ansiForeground, message, AnsiReset);
        }

        /// <summary>
        /// Converts a UnityEngine.Color to an ANSI escape code for foreground color.
        /// </summary>
        public static string AnsiToForeground(Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255);
            int g = Mathf.RoundToInt(color.g * 255);
            int b = Mathf.RoundToInt(color.b * 255);
            return $"\x1b[38;2;{r};{g};{b}m";
        }

        /// <summary>
        /// Converts a UnityEngine.Color to an ANSI escape code for background color.
        /// </summary>
        public static string AnsiBackground(Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255);
            int g = Mathf.RoundToInt(color.g * 255);
            int b = Mathf.RoundToInt(color.b * 255);
            return $"\x1b[48;2;{r};{g};{b}m";
        }

        /// <summary>
        /// ANSI reset code to restore default console color.
        /// </summary>
        public const string AnsiReset = "\x1b[0m";
    }
}
