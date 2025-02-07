using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Enums
{
    public sealed class CompositeLogCategory : LogCategory
    {
        internal readonly HashSet<LogCategory> Set;

        public bool IsEmpty => Set.Count == 0;

        internal CompositeLogCategory(LogCategory a, LogCategory b) : base("composite", false)
        {
            Set = new HashSet<LogCategory>();

            addToSet(a);
            addToSet(b);
        }

        private void addToSet(LogCategory category)
        {
            if (category == null) return;

            var composite = category as CompositeLogCategory;

            //The Set should not be allowed to contain other composites, only include what is contained with the set of the composite 
            if (composite != null)
                Set.UnionWith(composite.Set);
            else
                Set.Add(category);
        }

        /// <summary>
        /// Breaks the composite ExtEnum back into its component elements
        /// </summary>
        public LogCategory[] Deconstruct()
        {
            LogCategory[] elements = new LogCategory[Set.Count];
            Set.CopyTo(elements);
            return elements;
        }

        /// <summary>
        /// Checks whether this instance contains the specified flag element
        /// </summary>
        /// <param name="flag">The element to look for</param>
        /// <returns>true, if the flag element is contained by this instance; otherwise, false</returns>
        public bool Contains(LogCategory flag)
        {
            return Set.Contains(flag);
        }

        /// <summary>
        /// Checks whether this instance contains all elements specified within the composite flag
        /// </summary>
        /// <param name="flags">Contains a set of flags to look for</param>
        /// <returns>true, if all flag elements are contained by this instance; otherwise, false</returns>
        public bool Contains(CompositeLogCategory flags)
        {
            return HasAll(flags);
        }

        /// <summary>
        /// Checks whether this instance contains every element of another compatible composite instance
        /// </summary>
        /// <param name="flags">Contains a set of flags to look for</param>
        /// <returns>true, if all flag elements are contained by this instance; otherwise, false</returns>
        public bool HasAll(CompositeLogCategory flags)
        {
            if (flags == null || flags.IsEmpty) return false;

            return Set.IsSupersetOf(flags.Set);
        }

        /// <summary>
        /// Checks whether this instance contains any element of another compatible composite instance
        /// </summary>
        /// <param name="flags">Contains a set of flags to look for</param>
        /// <returns>true, if at least one flag element is contained by this instance; otherwise, false</returns>
        public bool HasAny(CompositeLogCategory flags)
        {
            if (flags == null || flags.IsEmpty) return false;

            return Set.Overlaps(flags.Set);
        }

        #region Object inherited methods
        public override int GetHashCode()
        {
            //TODO: This might be a bad idea, and can produce duplicate hashes
            return Set.Sum(element => element.GetHashCode());
        }

        public override string ToString()
        {
            return string.Join(" | ", Set);
        }
        #endregion
    }
}
