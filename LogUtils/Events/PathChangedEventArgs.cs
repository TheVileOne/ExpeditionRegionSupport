using System;

namespace LogUtils.Events
{
    public class PathChangedEventArgs : EventArgs
    {
        public readonly string NewPath;
        public readonly string OldPath;

        public PathChangedEventArgs(string newPath, string oldPath)
        {
            NewPath = newPath;
            OldPath = oldPath;
        }
    }
}
