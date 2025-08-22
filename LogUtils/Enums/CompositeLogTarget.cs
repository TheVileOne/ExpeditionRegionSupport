using LogUtils.Requests;
using System.Collections.Generic;

namespace LogUtils.Enums
{
    public class CompositeLogTarget : ILogTarget
    {
        /// <inheritdoc/>
        public string Value => string.Empty;

        /// <inheritdoc/>
        /// <remarks>Composite targets are never available to be handled</remarks>
        public bool IsEnabled => false;

        internal static readonly HashSet<ILogTarget> EmptySet = new HashSet<ILogTarget>();

        /// <summary>
        /// Contains the flags that represent the composite instance
        /// </summary>
        internal readonly HashSet<ILogTarget> Set = EmptySet;

        internal CompositeLogTarget()
        {
        }

        internal CompositeLogTarget(HashSet<ILogTarget> elements)
        {
            if (elements != null)
                Set = elements;
        }

        /// <inheritdoc/>
        public RequestType GetRequestType(ILogFileHandler handler)
        {
            return RequestType.Invalid; //Request system is not designed to handle composite targets
        }

        public LogTargetCollection ToCollection()
        {
            return new LogTargetCollection(this);
        }

        public static CompositeLogTarget operator |(CompositeLogTarget a, ILogTarget b)
        {
            return LogTarget.Combiner.Combine(a, b);
        }
    }
}
