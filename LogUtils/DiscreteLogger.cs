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

        /// <summary>
        /// Constructs a new <see cref="DiscreteLogger"/> instance
        /// </summary>
        /// <inheritdoc select="params"/>
        public DiscreteLogger(ILogTarget preset) : base(LoggingMode.Inherit, true, preset)
        {
        }

        /// <inheritdoc cref="DiscreteLogger(ILogTarget)"/>
        /// <inheritdoc select="params"/>
        public DiscreteLogger(bool allowLogging, ILogTarget preset) : base(LoggingMode.Inherit, allowLogging, preset)
        {
        }

        /// <inheritdoc cref="DiscreteLogger(ILogTarget)"/>
        /// <inheritdoc select="params"/>
        public DiscreteLogger(LoggingMode mode, ILogTarget preset) : base(mode, true, preset)
        {
        }

        /// <inheritdoc cref="DiscreteLogger(ILogTarget)"/>
        /// <inheritdoc select="params"/>
        public DiscreteLogger(LoggingMode mode, bool allowLogging, ILogTarget preset) : base(mode, allowLogging, preset)
        {
        }

        /// <inheritdoc cref="DiscreteLogger(ILogTarget)"/>
        /// <inheritdoc select="params"/>
        public DiscreteLogger(params ILogTarget[] presets) : base(LoggingMode.Inherit, true, presets)
        {
        }

        /// <inheritdoc cref="DiscreteLogger(ILogTarget)"/>
        /// <inheritdoc select="params"/>
        public DiscreteLogger(bool allowLogging, params ILogTarget[] presets) : base(LoggingMode.Inherit, allowLogging, presets)
        {
        }

        /// <inheritdoc cref="DiscreteLogger(ILogTarget)"/>
        /// <inheritdoc select="params"/>
        public DiscreteLogger(LoggingMode mode, params ILogTarget[] presets) : base(mode, true, presets)
        {
        }

        /// <inheritdoc cref="DiscreteLogger(ILogTarget)"/>
        /// <inheritdoc select="params"/>
        public DiscreteLogger(LoggingMode mode, bool allowLogging, params ILogTarget[] presets) : base(mode, allowLogging, presets)
        {
        }
    }
}
