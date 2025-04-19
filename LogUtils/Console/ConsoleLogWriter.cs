using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Requests;
using System;
using System.IO;
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
            try
            {
                Color messageColor = messageData.Category.ConsoleColor;
                if (LogConsole.ANSIColorSupport)
                {
                    string message = ApplyColorFormatting(ApplyRules(messageData), messageColor);
                    Stream.WriteLine(message);
                }
                else
                {
                    string message = ApplyRules(messageData);

                    LogConsole.SetConsoleColor(ConsoleColorMap.ClosestConsoleColor(messageColor));
                    Stream.WriteLine(message);
                    LogConsole.SetConsoleColor(ConsoleColor.Gray);
                }
            }
            catch (Exception ex)
            {
                //TODO: Report
            }
        }

        public void WriteFrom(LogRequest request)
        {
            SendToConsole(request.Data);

            if (request.Type == RequestType.Console)
                request.Complete();
        }

        public void WriteFromBuffer(LogID logFile, TimeSpan waitTime, bool respectBufferState = true)
        {
            throw new NotImplementedException();
        }
    }
}
