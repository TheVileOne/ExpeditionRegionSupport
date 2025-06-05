using LogUtils.Enums;
using LogUtils.Events;
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
        /// The active write stream for writers that use one
        /// </summary>
        public readonly TextWriter Stream;

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

        public string ApplyColorFormatting(string message, Color messageColor)
        {
            //Convert Unity color data to an ANSI escape code
            string ansiForeground = AnsiColorConverter.AnsiToForeground(messageColor);

            //Build the message string with ANSI code prepended and a reset at the end
            return string.Concat(ansiForeground, message, AnsiColorConverter.AnsiReset);
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
                message = ApplyColorFormatting(message, messageColor);
                Stream.WriteLine(message);
            }
            else
            {
                LogConsole.SetConsoleColor(ConsoleColorMap.ClosestConsoleColor(messageColor));
                Stream.WriteLine(message);
                LogConsole.SetConsoleColor(ConsoleColor.Gray);
            }
        }

        public void WriteFrom(LogRequest request)
        {
            try
            {
                if (!IsEnabled)
                {
                    request.Reject(RejectionReason.LogDisabled, ID);
                    return;
                }

                SendToConsole(request.Data);
            }
            catch (Exception ex)
            {
                request.Reject(RejectionReason.FailedToWrite, ID);
                UtilityLogger.LogError("Console write error", ex);
            }
            finally
            {
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
