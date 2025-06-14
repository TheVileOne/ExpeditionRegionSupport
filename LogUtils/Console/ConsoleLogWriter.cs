﻿using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Properties.Formatting;
using LogUtils.Requests;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LogUtils.Console
{
    public class ConsoleLogWriter : ILogWriter
    {
        public readonly ConsoleID ID;

        public bool IsEnabled;

        private LogMessageFormatter _formatter;

        public LogMessageFormatter Formatter
        {
            get => _formatter ?? LogMessageFormatter.Default;
            set => _formatter = value;
        }

        /// <summary>
        /// The message format rules associated with this writer
        /// </summary>
        public LogRuleCollection Rules = new LogRuleCollection();

        /// <summary>
        /// The active write stream for writers that use one
        /// </summary>
        public readonly TextWriter Stream;

        public uint TotalMessagesLogged;

        public ConsoleLogWriter(ConsoleID consoleID, TextWriter stream)
        {
            ID = consoleID;
            Stream = stream;

            IsEnabled = Stream != null;
        }

        public string ApplyRules(LogMessageEventArgs messageData)
        {
            return Formatter.Format(messageData);
        }

        public void SendToBuffer(LogMessageEventArgs messageData)
        {
            throw new NotImplementedException();
        }

        protected virtual void SendToConsole(LogMessageEventArgs messageData)
        {
            string message = ApplyRules(messageData);
            Color messageColor = messageData.Category.ConsoleColor;

            if (LogConsole.ANSIColorSupport)
            {
                message = AnsiColorConverter.ApplyFormat(message, messageColor);
                Stream.WriteLine(message);
            }
            else
            {
                LogConsole.SetConsoleColor(ConsoleColorMap.ClosestConsoleColor(messageColor));
                Stream.WriteLine(message);
                LogConsole.SetConsoleColor(ConsoleColorMap.DefaultConsoleColor);
            }
        }

        public void WriteFrom(LogRequest request)
        {
            ConsoleRequestEventArgs consoleMessageData = null;

            try
            {
                if (!IsEnabled)
                {
                    request.Reject(RejectionReason.LogDisabled, ID);
                    return;
                }

                consoleMessageData = request.Data.GetConsoleData();

                if (consoleMessageData == null)
                    request.Data.ExtraArgs.Add(consoleMessageData = new ConsoleRequestEventArgs(ID));

                consoleMessageData.TotalMessagesLogged = TotalMessagesLogged;
                consoleMessageData.Writer = this;

                SendToConsole(request.Data);
                TotalMessagesLogged++;
            }
            catch (Exception ex)
            {
                request.Reject(RejectionReason.FailedToWrite, ID);
                UtilityLogger.LogError("Console write error", ex);
            }
            finally
            {
                if (consoleMessageData != null)
                    consoleMessageData.Writer = null;

                request.NotifyComplete(ID);

                if (request.Type == RequestType.Console && !request.Data.PendingConsoleIDs.Any())
                    request.Complete();
            }
        }

        public void WriteFromBuffer(LogID logFile, TimeSpan waitTime, bool respectBufferState = true)
        {
            throw new NotImplementedException();
        }
    }
}
