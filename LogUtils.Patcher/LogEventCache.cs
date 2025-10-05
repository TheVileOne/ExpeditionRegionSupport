using BepInEx.Logging;
using System;
using System.Collections.Generic;
using LogEventData = (BepInEx.Logging.LogEventArgs EventData, System.DateTime Timestamp);

namespace LogUtils.VersionLoader;

internal class LogEventCache : ILogListener
{
    public List<LogEventData> Cache = new List<LogEventData>();

    public bool IsEnabled = true;

    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        if (!IsEnabled || eventArgs.Source != Patcher.Logger)
            return;

        Cache.Add(new LogEventData(eventArgs, DateTime.Now));
    }

    public void Dispose()
    {
        Logger.Listeners.Remove(this);
    }
}
