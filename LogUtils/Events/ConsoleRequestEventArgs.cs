using LogUtils.Enums;
using System;
using System.Collections.Generic;

namespace LogUtils.Events
{
    public class ConsoleRequestEventArgs : EventArgs
    {
        /// <summary>
        /// List of ConsoleIDs that were transfered from pending status
        /// </summary>
        public List<ConsoleID> Handled = new List<ConsoleID>();

        /// <summary>
        /// List of ConsoleIDs waiting to be handled
        /// </summary>
        public List<ConsoleID> Pending = new List<ConsoleID>();

        public ConsoleRequestEventArgs()
        {
        }

        public ConsoleRequestEventArgs(List<ConsoleID> pendingConsoleIDs)
        {
            Pending.AddRange(pendingConsoleIDs);
        }
    }
}
