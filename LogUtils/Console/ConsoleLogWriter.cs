using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Properties.Formatting;
using LogUtils.Requests;
using LogUtils.Requests.Validation;
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

        public bool ShowLogsAware;

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

        public string ApplyRules(LogRequestEventArgs messageData)
        {
            return Formatter.Format(messageData);
        }

        public void SendToBuffer(LogRequestEventArgs messageData)
        {
            throw new NotImplementedException();
        }

        protected virtual void SendToConsole(LogRequestEventArgs messageData)
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

                if (ShowLogsAware && !RainWorld.ShowLogs)
                {
                    request.Reject(RequestValidator.ShowLogsViolation(), ID);
                    return;
                }

                request.TargetConsole();
                consoleMessageData = request.SetDataFromWriter(this);

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
                request.ResetTarget();
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
