using LogUtils.Enums;
using System.Collections.Generic;

namespace LogUtils.Helpers.Extensions
{
    public static class HashSetExtensions
    {
        /// <summary>
        /// Ensures that set is not null, and invalid entries are not present
        /// <br>
        /// This method is null safe, and HashSet will be operated on directly
        /// </br>
        /// </summary>
        public static HashSet<LogCategory> Normalize(this HashSet<LogCategory> flags)
        {
            if (flags == null)
                return CompositeLogCategory.EmptySet;
            flags.Remove(LogCategory.None);
            return flags;
        }

        public static bool TryAdd(this HashSet<LogCategory> flags, LogCategory category)
        {
            if (category == null || category == LogCategory.None) return false;

            var composite = category as CompositeLogCategory;

            //The Set should not be allowed to contain other composites, only include what is contained with the set of the composite 
            if (composite != null)
                flags.UnionWith(composite.Set);
            else
                flags.Add(category);
            return true;
        }
    }
}
