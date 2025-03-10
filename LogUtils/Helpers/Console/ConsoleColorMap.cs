using LogUtils.Enums;
using System;
using UnityEngine;

namespace LogUtils.Helpers.Console
{
    public static class ConsoleColorMap
    {
        //TODO: Check that this default can apply to both BepInEx console and RainWorld console
        private readonly static ConsoleColor _defaultColor = ConsoleColor.Gray;

        /// <summary>
        /// The primary color used for text in the console
        /// </summary>
        public static Color DefaultColor => GetColor(_defaultColor);

        public static (ConsoleColor ConsoleColor, Color UnityColor)[] ColorMap =
        [
            (ConsoleColor.Black,       new Color(0f, 0f, 0f)),
            (ConsoleColor.DarkBlue,    new Color(0f, 0f, 0.5f)),
            (ConsoleColor.DarkGreen,   new Color(0f, 0.5f, 0f)),
            (ConsoleColor.DarkCyan,    new Color(0f, 0.5f, 0.5f)),
            (ConsoleColor.DarkRed,     new Color(0.5f, 0f, 0f)),
            (ConsoleColor.DarkMagenta, new Color(0.5f, 0f, 0.5f)),
            (ConsoleColor.DarkYellow,  new Color(0.5f, 0.5f, 0f)),
            (ConsoleColor.Gray,        new Color(0.75f, 0.75f, 0.75f)),
            (ConsoleColor.DarkGray,    new Color(0.5f, 0.5f, 0.5f)),
            (ConsoleColor.Blue,        new Color(0f, 0f, 1f)),
            (ConsoleColor.Green,       new Color(0f, 1f, 0f)),
            (ConsoleColor.Cyan,        new Color(0f, 1f, 1f)),
            (ConsoleColor.Red,         new Color(1f, 0f, 0f)),
            (ConsoleColor.Magenta,     new Color(1f, 0f, 1f)),
            (ConsoleColor.Yellow,      new Color(1f, 1f, 0f)),
            (ConsoleColor.White,       new Color(1f, 1f, 1f))
        ];

        /// <summary>
        /// Gets the Unity color mapped to a specified ConsoleColor code
        /// </summary>
        public static Color GetColor(ConsoleColor consoleColor)
        {
            int colorIndex = (int)consoleColor;

            if (colorIndex < 0 || colorIndex >= ColorMap.Length)
            {
                UtilityLogger.LogWarning("Console color code is invalid");
                colorIndex = (int)_defaultColor;
            }

            return ColorMap[colorIndex].UnityColor;
        }

        /// <summary>
        /// Gets the Unity color mapped to a specified LogGroup
        /// <br>Aligned with the colors assigned for BepInEx.LogLevel</br>
        /// </summary>
        public static Color GetColor(LogGroup group)
        {
            LogGroup mostRelevantGroup = group != LogGroup.All ? (LogGroup)FlagUtils.GetHighestBit((int)group) : group;

            ConsoleColor consoleColor = GetConsoleColor(mostRelevantGroup);
            return GetColor(consoleColor);
        }

        /// <summary>
        /// Gets the ConsoleColor mapped to a specified LogGroup
        /// <br>Aligned with the colors assigned for BepInEx.LogLevel</br>
        /// </summary>
        internal static ConsoleColor GetConsoleColor(LogGroup group)
        {
            return group switch
            {
                LogGroup.Fatal     => ConsoleColor.Red,
                LogGroup.Error     => ConsoleColor.DarkRed,
                LogGroup.Warning   => ConsoleColor.Yellow,
                LogGroup.Assert    => ConsoleColor.Yellow,
                LogGroup.Message   => ConsoleColor.White,
                LogGroup.Important => ConsoleColor.White,
                LogGroup.Debug     => ConsoleColor.DarkGray,
                LogGroup.All       => ConsoleColor.Cyan,
                _                  => _defaultColor
            };
        }
    }
}