using LogUtils.Console;
using LogUtils.Enums;
using LogUtils.Properties.Formatting;
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

        public ConsoleLogWriter Writer;

        /// <summary>
        /// The message format rules associated with this message data. Null when a writer isn't currently assigned
        /// </summary>
        public LogRuleCollection Rules => Writer?.Rules;

        public uint TotalMessagesLogged;

        public ConsoleRequestEventArgs()
        {
        }

        public ConsoleRequestEventArgs(ConsoleID pendingConsoleID)
        {
            Pending.Add(pendingConsoleID);
        }

        public ConsoleRequestEventArgs(List<ConsoleID> pendingConsoleIDs)
        {
            Pending.AddRange(pendingConsoleIDs);
        }
    }
}
