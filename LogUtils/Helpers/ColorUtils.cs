using UnityEngine;

namespace LogUtils.Helpers
{
    public static class ColorUtils
    {
        public static Color FromRGB(byte r, byte g, byte b)
        {
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        public static System.Drawing.Color ToSystemColor(this Color color)
        {
            return System.Drawing.Color.FromArgb((byte)(color.a * 255), (byte)(color.r * 255), (byte)(color.g * 255), (byte)(color.b * 255));
        }
    }
}
