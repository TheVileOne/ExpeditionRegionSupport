using BepInEx.Logging;
using System;
using System.IO;

namespace LogUtils.Patcher;

internal sealed class Logger : IDisposable
{
    private static readonly string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logutils.versionloader.log");
    private static readonly ManualLogSource _logSource = new ManualLogSource("LogUtils.VersionLoader");

    private bool isDisposed;

    public Logger()
    {
        if (File.Exists(_logPath))
            File.Delete(_logPath);

        BepInEx.Logging.Logger.Sources.Add(_logSource);
    }

    public void LogFatal(object message)
    {
        Log(LogLevel.Fatal, message);
    }

    public void LogError(object message)
    {
        Log(LogLevel.Error, message);
    }

    public void LogWarning(object message)
    {
        Log(LogLevel.Warning, message);
    }

    public void LogMessage(object message)
    {
        Log(LogLevel.Message, message);
    }

    public void LogInfo(object message)
    {
        Log(LogLevel.Info, message);
    }

    public void LogDebug(object message)
    {
        Log(LogLevel.Debug, message);
    }

    internal void Log(LogLevel level, object message)
    {
        _logSource.Log(level, message);

        try
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";
            File.AppendAllText(_logPath, line);
        }
        catch (Exception ex)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - !!!Exception: {ex}{Environment.NewLine}";
            File.AppendAllText(_logPath, line);
        }
    }

    #region Dispose pattern
    internal void Dispose(bool disposing)
    {
        if (isDisposed) return;

        isDisposed = true;
        BepInEx.Logging.Logger.Sources.Remove(_logSource);
    }

    ~Logger()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
