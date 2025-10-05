using LogUtils.Console;
using LogUtils.Enums;
using LogUtils.Helpers;
using System;
using UnityEngine;
using SystemColor = System.Drawing.Color;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal static class LogConsoleTests
    {
        /// <summary>
        /// Sends color debug information to the console
        /// </summary>
        public static void TestColors()
        {
            ConsoleLogWriter console = findConsole();

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

                        console.Stream.Write(AnsiColorConverter.ApplyFormat(systemColor.ToString(), color));

                        //Sets a second color on the same line - this code intentionally does not use ANSI to set the color
                        LogConsole.SetConsoleColor(consoleColor);
                        console.Stream.WriteLine(" " + consoleColor + " Color Code: " + unityConsoleColor.ToSystemColor());
                        LogConsole.SetConsoleColor(ConsoleColorMap.DefaultConsoleColor);
                    }
                }
            }
        }

        public static void TestColorConsistency()
        {
            ConsoleLogWriter console = findConsole();

            if (console == null)
                return;

            foreach (ConsoleColor colorValue in Enum.GetValues(typeof(ConsoleColor)))
            {
                //Show that color helpers return correct values, and the console produces the correct color
                LogConsole.SetConsoleColor(colorValue);
                console.Stream.WriteLine(colorValue);

                var unityColor = ConsoleColorMap.GetColor(colorValue);
                LogConsole.SetConsoleColor(ConsoleColorMap.ClosestConsoleColor(unityColor));
                console.Stream.WriteLine("Unity Color");
                console.Stream.WriteLine(AnsiColorConverter.ApplyFormat("ANSI Color", unityColor));
            }
            LogConsole.SetConsoleColor(ConsoleColorMap.DefaultConsoleColor);
        }

        private static ConsoleLogWriter findConsole() => LogConsole.FindWriter(ConsoleID.BepInEx, enabledOnly: true);
    }
}
