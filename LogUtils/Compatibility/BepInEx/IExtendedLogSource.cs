using BepInEx.Logging;

namespace LogUtils.Compatibility.BepInEx
{
    /// <summary>
    /// Represents a type that implements both ILogger, and ILogSource
    /// </summary>
    public interface IExtendedLogSource : ILogger, ILogSource;
}
