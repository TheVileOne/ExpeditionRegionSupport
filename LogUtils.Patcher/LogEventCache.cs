using BepInEx.Logging;
using System;
using System.Collections.Generic;
using LogEventData = (BepInEx.Logging.LogEventArgs EventData, System.DateTime Timestamp);

namespace LogUtils.Patcher;

internal class LogEventCache : ILogListener
{
    public List<LogEventData> Cache = new List<LogEventData>();

    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        if (eventArgs.Source == Patcher.Logger)
            Cache.Add(new LogEventData(eventArgs, DateTime.Now));
    }

    public void Dispose()
    {
        Logger.Listeners.Remove(this);
    }
}
