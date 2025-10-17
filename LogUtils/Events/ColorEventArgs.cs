using System;
using UnityEngine;

namespace LogUtils.Events
{
    public class ColorEventArgs : EventArgs
    {
        public readonly Color Color;

        public ColorEventArgs(Color color)
        {
            Color = color;
        }
    }
}
