using LogUtils.Enums;
using LogUtils.Helpers;
using System;
using UnityEngine;
using SystemColor = System.Drawing.Color;

namespace LogUtils.Console
{
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
        /// Surrounds a message withANSI codes necessary to display the message in the console with a specified color, and reset back to the default color at the end of the message
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

        /// <summary>
        /// Sends color debug information to the console
        /// </summary>
        internal static void TestColors()
        {
            ConsoleLogWriter console = LogConsole.FindWriter(ConsoleID.BepInEx, enabledOnly: true);

            if (console == null)
                return;

            var fixedValue = new
            {
                R = -1,
                G = -1,
                B = -1
            };

            short incrValue = 2;
            for (short r = 0; r <= 255; r += incrValue)
            {
                if (fixedValue.R >= 0 && r != fixedValue.R)
                    continue;

                for (short g = 0; g <= 255; g += incrValue)
                {
                    if (fixedValue.G >= 0 && g != fixedValue.G)
                        continue;

                    for (short b = 0; b <= 255; b += incrValue)
                    {
                        if (fixedValue.B >= 0 && b != fixedValue.B)
                            continue;

                        Color color = ColorUtils.FromRGB((byte)r, (byte)g, (byte)b);
                        SystemColor systemColor = SystemColor.FromArgb(255, r, g, b);

                        ConsoleColor consoleColor = ConsoleColorMap.ClosestConsoleColor(color);

                        Color unityConsoleColor = ConsoleColorMap.GetColor(consoleColor);

                        console.Stream.Write(ApplyFormat(systemColor.ToString(), color));

                        //Sets a second color on the same line - this code intentionally does not use ANSI to set the color
                        LogConsole.SetConsoleColor(consoleColor);
                        console.Stream.WriteLine(" " + consoleColor + " Color Code: " + unityConsoleColor.ToSystemColor());
                        LogConsole.SetConsoleColor(ConsoleColorMap.DefaultConsoleColor);
                    }
                }
            }
        }
    }
}
