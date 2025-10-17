using System;

namespace LogUtils.Compatibility.Unity
{
    public class UnityLogEventArgs : EventArgs
    {
        /// <summary>
        /// Unity object - typically given to provide context to the log message
        /// </summary>
        public readonly UnityEngine.Object Context;

        /// <summary>
        /// Unity tag - typically given to provide context to the log message
        /// </summary>
        public readonly string Tag;

        public UnityLogEventArgs(UnityEngine.Object context, string tag)
        {
            Context = context;
            Tag = tag;
        }
    }
}
