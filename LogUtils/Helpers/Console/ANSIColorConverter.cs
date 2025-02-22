using UnityEngine;

namespace LogUtils.Helpers.Console
{
    public static class AnsiColorConverter
    {
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
