﻿using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Requests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Diagnostics.Tests.Components
{
    public class FakeLogWriter : ILogWriter
    {
        public List<LogRequest> ReceivedRequests = new List<LogRequest>();

        public LogRequest LatestRequest => ReceivedRequests.LastOrDefault();

        string ILogWriter.ApplyRules(LogMessageEventArgs logEventData)
        {
            //logEventData.MessageFormatted = logEventData.Message;
            return logEventData.Message;
        }

        void ILogWriter.WriteFrom(LogRequest request)
        {
            ReceivedRequests.Add(request);
        }

        void IBufferHandler.SendToBuffer(LogMessageEventArgs messageData)
        {
            //Does nothing
        }

        void IBufferHandler.WriteFromBuffer(LogID logFile, TimeSpan waitTime, bool respectBufferState = true)
        {
            //Does nothing
        }
    }
}
