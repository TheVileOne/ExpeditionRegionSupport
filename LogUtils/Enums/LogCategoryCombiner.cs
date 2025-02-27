using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Enums
{
    public class LogCategoryCombiner
    {
        private HashSet<LogCategory> set;

        /// <summary>
        /// Create a composite object out of two component objects
        /// </summary>
        public CompositeLogCategory Combine(LogCategory a, LogCategory b)
        {
            set = new HashSet<LogCategory>();

            //All takes priority over all other elements
            if (a == LogCategory.All || b == LogCategory.All)
            {
                addToSet(LogCategory.All);
            }
            else
            {
                addToSet(a);
                addToSet(b);
            }
            return new CompositeLogCategory(set);
        }

        public CompositeLogCategory Intersect(LogCategory a, LogCategory b)
        {
            //There cannot be elements in common if one of the options is null
            if (a == null || b == null || a == LogCategory.None || b == LogCategory.None)
                return CompositeLogCategory.Empty;

            set = new HashSet<LogCategory>();

            if (a == b)
            {
                //It doesn't matter which option is used - both contain the same elements
                addToSet(a);
                return new CompositeLogCategory(set);
            }

            if (a == LogCategory.All || b == LogCategory.All)
            {
                if (a == LogCategory.All)
                    addToSet(b);
                else
                    addToSet(a);
                return new CompositeLogCategory(set);
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
                        set.Add(b);
                }
                else if (compositeOther != null)
                {
                    if (compositeOther.Contains(a))
                        set.Add(a);
                }
                return new CompositeLogCategory(set);
            }

            //Use Intersect to find the common elements between the two sets
            set.UnionWith(composite.Set.Intersect(compositeOther.Set));
            return new CompositeLogCategory(set);
        }

        public CompositeLogCategory Distinct(LogCategory a, LogCategory b)
        {
            if (a == b)
                return CompositeLogCategory.Empty;

            set = new HashSet<LogCategory>();

            //When one option contains no elements, the other option can be directly added to the set
            if (a == null || a == LogCategory.None)
            {
                addToSet(b);
            }
            else if (b == null || b == LogCategory.None)
            {
                addToSet(a);
            }
            else if (a == LogCategory.All || b == LogCategory.All) //a does not equal b
            {
                LogCategory category = a == LogCategory.All ? b : a;
                var excludeList = extractElements(category);

                set.UnionWith(LogCategory.RegisteredEntries.Except(excludeList));
            }
            else
            {
                var composite = a as CompositeLogCategory;
                var compositeOther = b as CompositeLogCategory;

                //Find all of the distinct elements
                if (composite == null || compositeOther == null)
                {
                    if (composite != null)
                        set.UnionWith(composite.Set.Except([b]));
                    else if (compositeOther != null)
                        set.UnionWith(compositeOther.Set.Except([a]));
                    else
                    {
                        //When both composite casts fail, both options must be distinct, because a does not equal b
                        set.Add(a);
                        set.Add(b);
                    }
                    return new CompositeLogCategory(set);
                }

                //This ugly beast is how you get the distinct elements from two collections
                set.UnionWith(composite.Set.Except(compositeOther.Set).Union(compositeOther.Set.Except(composite.Set)));
            }
            return new CompositeLogCategory(set);
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

            set = new HashSet<LogCategory>(LogCategory.RegisteredEntries.Except(excludeList));
            return new CompositeLogCategory(set);
        }

        private void addToSet(LogCategory category)
        {
            if (category == null || category == LogCategory.None) return;

            var composite = category as CompositeLogCategory;

            //The Set should not be allowed to contain other composites, only include what is contained with the set of the composite 
            if (composite != null)
                set.UnionWith(composite.Set);
            else
                set.Add(category);
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
