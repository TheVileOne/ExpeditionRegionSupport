using LogUtils.Enums;
using System.Collections.Generic;

namespace LogUtils
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Ensures that set is not null, and invalid entries are not present
        /// </summary>
        /// <remarks>This method is null safe, and HashSet will be operated on directly</remarks>
        public static HashSet<LogCategory> Normalize(this HashSet<LogCategory> flags)
        {
            if (flags == null)
                return CompositeLogCategory.EmptySet;
            flags.Remove(LogCategory.None);
            return flags;
        }

        public static bool TryAdd(this HashSet<ILogTarget> flags, ILogTarget value)
        {
            if (value == null) return false;

            var composite = value as CompositeLogTarget;

            //Set should not be allowed to contain other composites, only include what is contained with the set of the composite 
            if (composite != null)
                flags.UnionWith(composite.Set);
            else
                flags.Add(value);
            return true;
        }

        public static bool TryAdd(this HashSet<LogCategory> flags, LogCategory value)
        {
            if (value == null || value == LogCategory.None) return false;

            var composite = value as CompositeLogCategory;

            //Set should not be allowed to contain other composites, only include what is contained with the set of the composite 
            if (composite != null)
                flags.UnionWith(composite.Set);
            else
                flags.Add(value);
            return true;
        }
    }
}
