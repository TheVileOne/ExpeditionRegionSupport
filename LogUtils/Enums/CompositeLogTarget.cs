using LogUtils.Requests;
using System.Collections.Generic;

namespace LogUtils.Enums
{
    public class CompositeLogTarget : ILogTarget
    {
        public string Value => string.Empty;

        public bool IsEnabled => false; //Composite targets are never available to be handled

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

        public RequestType GetRequestType(ILogHandler handler)
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
