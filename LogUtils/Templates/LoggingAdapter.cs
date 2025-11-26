using BepInEx.Logging;
using LogUtils.Enums;
using System;

namespace LogUtils.Templates
{
    /// <summary>
    /// An example class demonstration on how to encapsulate, and use a LogUtils instance without the assembly being aware that LogUtils is available at runtime.
    /// A fallback implementation is used in the case that LogUtils is unavailable. Be aware that a fallback implementation should bear the same responsibilities
    /// associated with maintaining a custom log file in the situation that LogUtils cannot perform such functions for the user.
    /// </summary>
    public static class LoggingAdapter
    {
        private static class LogUtilsAccess
        {
            /// <summary>
            /// Attempt to initialize LogUtils assembly
            /// </summary>
            /// <exception cref="TypeLoadException">An assembly dependency is unavailable, or is of the wrong version</exception>
            internal static void UnsafeAccess()
            {
                UtilityCore.EnsureInitializedState();
            }

            internal static IMyLogger CreateLogger()
            {
                UnsafeAccess();

                //These represent the log files you want to target for logging
                var myLogTargets = LogID.BepInEx | LogID.Unity;

                LogUtilsAdapter adapter = new LogUtilsAdapter()
                {
                    Logger = new Logger(myLogTargets)
                };
                return adapter;
            }

            /// <summary>
            /// Wrapper class for a LogUtils Logger instance
            /// </summary>
            internal class LogUtilsAdapter : IMyLogger
            {
                internal ILogger Logger;

                void IMyLogger.Log(object message)
                {
                    Logger.Log(message);
                }

                void IMyLogger.Log(LogLevel category, object message)
                {
                    Logger.Log(category, message);
                }
            }
        }

        /// <summary>
        /// Creates a logger employing a safe encapsulation technique
        /// </summary>
        internal static IMyLogger CreateLogger()
        {
            try
            {
                return LogUtilsAccess.CreateLogger();
            }
            catch //Caught exception will probably be a TypeLoadException
            {
                return new FallbackLogger();
            }
        }
    }

    /// <summary>
    /// Interface provides a safe boundary with and a compatible interface for a LogUtils logger instance
    /// </summary>
    public interface IMyLogger
    {
        void Log(object message);
        void Log(LogLevel category, object message);
    }

    public class FallbackLogger : IMyLogger
    {
        private static ManualLogSource logSource;

        static FallbackLogger()
        {
            logSource = BepInEx.Logging.Logger.CreateLogSource("MyMod");
        }

        public void Log(object message)
        {
            logSource.Log(LogLevel.Info, message);
        }

        public void Log(LogLevel category, object message)
        {
            logSource.Log(category, message);
        }
    }
}
