using System;
using UnityEngine;

namespace LogUtils.Events
{
    public class ColorEventArgs : EventArgs
    {
        public Color Color { get; }

        public ColorEventArgs(Color color)
        {
            Color = color;
        }
    }
}
