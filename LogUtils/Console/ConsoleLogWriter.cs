﻿using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Formatting;
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

        private LogMessageFormatter _formatter;

        /// <summary>
        /// Applies text-based message format changes
        /// </summary>
        public LogMessageFormatter Formatter
        {
            get => _formatter ?? LogMessageFormatter.Default;
            set => _formatter = value;
        }

        public bool IsEnabled;

        /// <summary>
        /// Collection of associated message format rules
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

            if (ID == ConsoleID.BepInEx)
            {
                Formatter = new LogMessageFormatter(new AnsiColorFormatProvider());
                Rules = LogID.BepInEx.Properties.Rules;
            }

            IsEnabled = Stream != null;
        }

        /// <inheritdoc/>
        public string ApplyRules(LogRequestEventArgs messageData)
        {
            return Formatter.Format(messageData);
        }

        /// <inheritdoc/>
        public void SendToBuffer(LogRequestEventArgs messageData)
        {
            throw new NotImplementedException();
        }

        protected virtual void SendToConsole(LogRequestEventArgs messageData)
        {
            IColorFormatProvider colorProvider = Formatter.ColorFormatter;

            Color lastDefaultColor = colorProvider.MessageColor ?? ConsoleColorMap.DefaultColor; ;
            try
            {
                Color messageColor = messageData.MessageColor;
                colorProvider.MessageColor = messageColor;
                string message = ApplyRules(messageData);

                if (LogConsole.ANSIColorSupport)
                {
                    message = Formatter.ApplyColor(message, messageColor);
                    Stream.WriteLine(message);
                    Stream.Write(AnsiColorConverter.AnsiToForeground(lastDefaultColor));
                }
                else
                {
                    LogConsole.SetConsoleColor(ConsoleColorMap.ClosestConsoleColor(messageColor));
                    Stream.WriteLine(message);
                    LogConsole.SetConsoleColor(ConsoleColorMap.ClosestConsoleColor(lastDefaultColor));
                }
            }
            finally
            {
                //Maintain a null default instead of inheriting the default console color. That way in the future if the default console color changes, the provider
                //wont remain at the old default color
                colorProvider.MessageColor = lastDefaultColor != ConsoleColorMap.DefaultColor ? lastDefaultColor : null;
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void WriteFromBuffer(LogID logFile, TimeSpan waitTime, bool respectBufferState = true)
        {
            throw new NotImplementedException();
        }
    }
}
