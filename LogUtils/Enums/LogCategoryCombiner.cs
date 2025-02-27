using LogUtils.Helpers.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Enums
{
    public class LogCategoryCombiner
    {
        /// <summary>
        /// Create a composite object out of two component objects
        /// </summary>
        public CompositeLogCategory Combine(LogCategory a, LogCategory b)
        {
            var flags = new HashSet<LogCategory>();

            //All takes priority over all other elements
            if (a == LogCategory.All || b == LogCategory.All)
            {
                flags.TryAdd(LogCategory.All);
            }
            else
            {
                flags.TryAdd(a);
                flags.TryAdd(b);
            }
            return new CompositeLogCategory(flags);
        }

        public CompositeLogCategory Intersect(LogCategory a, LogCategory b)
        {
            //There cannot be elements in common if one of the options is null
            if (a == null || b == null || a == LogCategory.None || b == LogCategory.None)
                return CompositeLogCategory.Empty;

            var flags = new HashSet<LogCategory>();

            if (a == b)
            {
                //It doesn't matter which option is used - both contain the same elements
                flags.TryAdd(a);
                return new CompositeLogCategory(flags);
            }

            if (a == LogCategory.All || b == LogCategory.All)
            {
                if (a == LogCategory.All)
                    flags.TryAdd(b);
                else
                    flags.TryAdd(a);
                return new CompositeLogCategory(flags);
            }

            var composite = a as CompositeLogCategory;
            var compositeOther = b as CompositeLogCategory;

            //Find all of the common elements
            if (composite == null || compositeOther == null)
            {
                //When both composite casts fail, the options cannot have elements in common, because a does not equal b
                if (composite != null)
                {
                    if (composite.Contains(b))
                        flags.Add(b);
                }
                else if (compositeOther != null)
                {
                    if (compositeOther.Contains(a))
                        flags.Add(a);
                }
                return new CompositeLogCategory(flags);
            }

            //Use Intersect to find the common elements between the two sets
            flags.UnionWith(composite.Set.Intersect(compositeOther.Set));
            return new CompositeLogCategory(flags);
        }

        public CompositeLogCategory Distinct(LogCategory a, LogCategory b)
        {
            if (a == b)
                return CompositeLogCategory.Empty;

            var flags = new HashSet<LogCategory>();

            //When one option contains no elements, the other option can be directly added to the set
            if (a == null || a == LogCategory.None)
            {
                flags.TryAdd(b);
            }
            else if (b == null || b == LogCategory.None)
            {
                flags.TryAdd(a);
            }
            else if (a == LogCategory.All || b == LogCategory.All) //a does not equal b
            {
                LogCategory category = a == LogCategory.All ? b : a;
                var excludeList = extractElements(category);

                flags.UnionWith(LogCategory.RegisteredEntries.Except(excludeList));
            }
            else
            {
                var composite = a as CompositeLogCategory;
                var compositeOther = b as CompositeLogCategory;

                //Find all of the distinct elements
                if (composite == null || compositeOther == null)
                {
                    if (composite != null)
                        flags.UnionWith(composite.Set.Except([b]));
                    else if (compositeOther != null)
                        flags.UnionWith(compositeOther.Set.Except([a]));
                    else
                    {
                        //When both composite casts fail, both options must be distinct, because a does not equal b
                        flags.Add(a);
                        flags.Add(b);
                    }
                    return new CompositeLogCategory(flags);
                }

                var distinctFlagsA = composite.Set.Except(compositeOther.Set);
                var distinctFlagsB = compositeOther.Set.Except(composite.Set);
                flags.UnionWith(distinctFlagsA.Union(distinctFlagsB));
            }
            return new CompositeLogCategory(flags);
        }

        public LogCategory GetComplement(LogCategory target)
        {
            if (target == null || target == LogCategory.None)
                return LogCategory.All;

            if (!target.Registered)
            {
                var composite = target as CompositeLogCategory;

                //The complement of an unregistered entry is the set of all registered entries
                if (composite == null || composite.Set.All(entry => !entry.Registered)) //Composites are unregistered by default, check composite flags instead
                    return LogCategory.All;
            }

            if (target == LogCategory.All)
                return LogCategory.None;

            var excludeList = extractElements(target);

            var flags = new HashSet<LogCategory>(LogCategory.RegisteredEntries.Except(excludeList));
            return new CompositeLogCategory(flags);
        }

        /// <summary>
        /// Extract the component elements from the provided category
        /// </summary>
        private List<LogCategory> extractElements(LogCategory category)
        {
            var elements = new List<LogCategory>();
            var composite = category as CompositeLogCategory;

            if (composite != null)
                elements.AddRange(composite.Set);
            else
                elements.Add(category);
            return elements;
        }
    }
}
