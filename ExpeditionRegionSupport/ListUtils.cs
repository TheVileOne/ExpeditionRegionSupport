using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpeditionRegionSupport
{
    public static class ListUtils
    {
        /// <summary>
        /// Removes all values from a list
        /// </summary>
        public static void RemoveAll<T>(this List<T> self, List<T> list)
        {
            if (list == null) return;

            foreach (T item in list)
                self.Remove(item);
        }

        public static T FindType<T>(this List<T> self, T item)
        {
            return self.Find(i => i.GetType().Equals(item.GetType()));
        }

        public static string FormatToString<T>(this IEnumerable<T> self, char separator, string stringIfEmpty = "NONE")
        {
            return self != null ? self.Any() ? string.Join(separator + " ", self) : stringIfEmpty : null;
        }

        public static bool Exists<T>(this IEnumerable<T> self, Func<T, bool> predicate)
        {
            T result = self.FirstOrDefault(predicate);

            return result?.Equals(default(T)) ?? false;
        }

        public class ConstrainedList<T> : List<T>
        {
            /// <summary>
            /// A condition to check against each object added to this list. True adds item to list, false rejects it.
            /// </summary>
            public Predicate<T> Constraint;

            public ConstrainedList(Predicate<T> constraint)
            {
                Constraint = constraint;
            }

            public new void Add(T item)
            {
                if (Constraint.Invoke(item))
                    base.Add(item);
            }
        }
    }
}
