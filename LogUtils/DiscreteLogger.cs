using LogUtils.Enums;

namespace LogUtils
{
    /// <summary>
    /// A logger instance designed to be isolated from remote requests by default
    /// </summary>
    public sealed class DiscreteLogger : Logger
    {
        public override bool AllowRemoteLogging => false;

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="presets">Include any LogIDs that this logger targets, or handles on request</param>
        public DiscreteLogger(params LogID[] presets) : this(LoggingMode.Inherit, true, presets)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="allowLogging">Whether logger accepts logs by default, or has to be enabled first</param>
        /// <param name="presets">Include any LogIDs that this logger targets, or handles on request</param>
        public DiscreteLogger(bool allowLogging, params LogID[] presets) : this(LoggingMode.Inherit, allowLogging, presets)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="mode">Changes the technique used to write messages to file</param>
        /// <param name="presets">Include any LogIDs that this logger targets, or handles on request</param>
        public DiscreteLogger(LoggingMode mode, params LogID[] presets) : this(mode, true, presets)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="mode">Changes the technique used to write messages to file</param>
        /// <param name="allowLogging">Whether logger accepts logs by default, or has to be enabled first</param>
        /// <param name="presets">Include any LogIDs that this logger targets, or handles on request</param>
        public DiscreteLogger(LoggingMode mode, bool allowLogging, params LogID[] presets) : base(mode, allowLogging, presets)
        {
        }
    }
}
