using LogUtils.Enums;

namespace LogUtils
{
    /// <summary>
    /// A logger instance designed to be isolated from remote requests by default
    /// </summary>
    public sealed class DiscreteLogger : Logger
    {
        /// <inheritdoc/>
        /// <value>Always returns false</value>
        public override bool AllowRemoteLogging => false;

        /// <inheritdoc/>
        public DiscreteLogger(params ILogTarget[] presets) : this(LoggingMode.Inherit, true, presets)
        {
        }

        /// <inheritdoc/>
        public DiscreteLogger(bool allowLogging, params ILogTarget[] presets) : this(LoggingMode.Inherit, allowLogging, presets)
        {
        }

        /// <inheritdoc/>
        public DiscreteLogger(LoggingMode mode, params ILogTarget[] presets) : this(mode, true, presets)
        {
        }

        /// <inheritdoc/>
        public DiscreteLogger(LoggingMode mode, bool allowLogging, params ILogTarget[] presets) : base(mode, allowLogging, presets)
        {
        }
    }
}
