using BepInEx.Logging;
using System;
using System.IO;

namespace LogUtils.Patcher;

internal sealed class Logger : IDisposable
{
    private static readonly string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logutils.versionloader.log");
    private static readonly ManualLogSource _logSource = new ManualLogSource("LogUtils.VersionLoader");

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

    #region Dispose handling

    /// <summary/>
    internal bool IsDisposed;

    /// <summary>
    /// Performs tasks for disposing a <see cref="Patcher.Logger"/>
    /// </summary>
    /// <param name="disposing">Whether or not the dispose request is invoked by the application (true), or invoked by the destructor (false)</param>
    internal void Dispose(bool disposing)
    {
        if (IsDisposed) return;

        IsDisposed = true;
        BepInEx.Logging.Logger.Sources.Remove(_logSource);
    }

    /// <inheritdoc cref="Dispose(bool)"/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary/>
    ~Logger()
    {
        Dispose(disposing: false);
    }
    #endregion
}
