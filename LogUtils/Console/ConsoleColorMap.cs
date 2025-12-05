using LogUtils.Enums;
using LogUtils.Helpers;
using System;
using UnityEngine;

namespace LogUtils.Console
{
    /// <summary>
    /// This class provides helper methods for converting <see cref="ConsoleColor"/> enums to their corresponding <see cref="Color"/> value, and vice versa
    /// </summary>
    public static class ConsoleColorMap
    {
        //TODO: Check that this default can apply to both BepInEx console and RainWorld console
        /// <summary>
        /// The primary color used for text in the console
        /// </summary>
        public static ConsoleColor DefaultConsoleColor = ConsoleColor.Gray;

        /// <summary>
        /// The primary color used for text in the console
        /// </summary>
        public static Color DefaultColor => GetColor(DefaultConsoleColor);

        /// <summary>
        /// A map of <see cref="ConsoleColor"/> values to their associated <see cref="Color"/> value
        /// </summary>
        #pragma warning disable IDE0055 //Fix formatting
        public static (ConsoleColor ConsoleColor, Color UnityColor)[] ColorMap =
        [
            (ConsoleColor.Black,       new Color(0f, 0f, 0f)),
            (ConsoleColor.DarkBlue,    new Color(0f, 0.24f, 0.86f)),
            (ConsoleColor.DarkGreen,   new Color(0f, 0.6f, 0f)),
            (ConsoleColor.DarkCyan,    new Color(0.16f, 0.61f, 0.88f)),
            (ConsoleColor.DarkRed,     new Color(0.75f, 0f, 0f)),
            (ConsoleColor.DarkMagenta, new Color(0.51f, 0f, 0.6f)),
            (ConsoleColor.DarkYellow,  new Color(0.82f, 0.63f, 0f)),
            (ConsoleColor.Gray,        new Color(0.75f, 0.75f, 0.75f)),
            (ConsoleColor.DarkGray,    new Color(0.5f, 0.5f, 0.5f)),
            (ConsoleColor.Blue,        new Color(0.16f, 0.46f, 1f)),
            (ConsoleColor.Green,       new Color(0.02f, 0.8f, 0f)),
            (ConsoleColor.Cyan,        new Color(0.31f, 0.79f, 0.78f)),
            (ConsoleColor.Red,         new Color(0.88f, 0.26f, 0.33f)),
            (ConsoleColor.Magenta,     new Color(0.67f, 0f, 0.54f)),
            (ConsoleColor.Yellow,      new Color(0.93f, 0.91f, 0.6f)),
            (ConsoleColor.White,       new Color(1f, 1f, 1f))
        ];
        #pragma warning restore IDE0055 //Fix formatting

        /// <summary>
        /// Gets the <see cref="Color"/> value mapped to a specified <see cref="ConsoleColor"/> code
        /// </summary>
        public static Color GetColor(ConsoleColor consoleColor)
        {
            int colorIndex = (int)consoleColor;

            if (colorIndex < 0 || colorIndex >= ColorMap.Length)
            {
                UtilityLogger.LogWarning("Console color code is invalid");
                colorIndex = (int)DefaultConsoleColor;
            }

            return ColorMap[colorIndex].UnityColor;
        }

        /// <summary>
        /// Gets the <see cref="Color"/> value mapped to a specified <see cref="LogCategoryLevels"/> value
        /// </summary>
        /// <remarks>Aligned with the colors assigned for <see cref="BepInEx.Logging.LogLevel"/></remarks>
        public static Color GetColor(LogCategoryLevels level)
        {
            LogCategoryLevels mostRelevantLevel = level != LogCategoryLevels.All ? (LogCategoryLevels)FlagUtils.GetHighestBit((int)level) : level;

            ConsoleColor consoleColor = GetConsoleColor(mostRelevantLevel);
            return GetColor(consoleColor);
        }

        /// <summary>
        /// Gets the <see cref="ConsoleColor"/> value mapped to a specified <see cref="LogCategoryLevels"/> value
        /// </summary>
        /// <remarks>Aligned with the colors assigned for <see cref="BepInEx.Logging.LogLevel"/></remarks>
        #pragma warning disable IDE0055 //Fix formatting
        internal static ConsoleColor GetConsoleColor(LogCategoryLevels level)
        {
            return level switch
            {
                LogCategoryLevels.Fatal     => ConsoleColor.Red,
                LogCategoryLevels.Error     => ConsoleColor.DarkRed,
                LogCategoryLevels.Warning   => ConsoleColor.Yellow,
                LogCategoryLevels.Assert    => ConsoleColor.Yellow,
                LogCategoryLevels.Message   => ConsoleColor.White,
                LogCategoryLevels.Important => ConsoleColor.White,
                LogCategoryLevels.Debug     => ConsoleColor.DarkGray,
                LogCategoryLevels.All       => ConsoleColor.Cyan,
                _                          => DefaultConsoleColor
            };
        }
        #pragma warning restore IDE0055 //Fix formatting

        /// <summary>
        /// Finds the nearest compatible <see cref="ConsoleColor"/> value for the given color
        /// </summary>
        public static ConsoleColor ClosestConsoleColor(Color color)
        {
            return ClosestConsoleColor((byte)(color.r * 255f), (byte)(color.g * 255f), (byte)(color.b * 255f));
        }

        /// <inheritdoc cref="ClosestConsoleColor(Color)"/>
        public static ConsoleColor ClosestConsoleColor(System.Drawing.Color color)
        {
            return ClosestConsoleColor(color.R, color.G, color.B);
        }

        /// <inheritdoc cref="ClosestConsoleColor(Color)"/>
        public static ConsoleColor ClosestConsoleColor(byte r, byte g, byte b)
        {
            double delta = double.MaxValue;

            ConsoleColor result = default;

            foreach (var colorEntry in ColorMap)
            {
                Color color = colorEntry.UnityColor;

                //Code sourced from https://stackoverflow.com/a/12340136/30273286
                double t = Math.Pow((color.r * 255f) - r, 2.0) + Math.Pow((color.g * 255f) - g, 2.0) + Math.Pow((color.b * 255f) - b, 2.0);

                if (t == 0.0) //Exact match
                {
                    result = colorEntry.ConsoleColor;
                    break;
                }

                if (t < delta)
                {
                    delta = t;
                    result = colorEntry.ConsoleColor;
                }
            }
            return result;
        }
    }
}