using LogUtils.Requests;

namespace LogUtils.Enums
{
    public static class LogTarget
    {
        public static ICombiner<ILogTarget, CompositeLogTarget> Combiner = new LogTargetCombiner();
    }

    /// <summary>
    /// Shared by implementations that function as a compatible target by a Logger instance
    /// </summary>
    public interface ILogTarget
    {
        /// <summary>
        /// A value string assigned on construction
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Indicates whether this target is available to be handled
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// The RequestType that should be assigned when this target is ready to be handled
        /// </summary>
        RequestType GetRequestType(ILogFileHandler handler);
    }
}
