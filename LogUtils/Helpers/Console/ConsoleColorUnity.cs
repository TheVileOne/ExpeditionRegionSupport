using System;
using UnityEngine;

namespace LogUtils.Helpers.Console
{
    public static class ConsoleColorUnity
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
    }
}