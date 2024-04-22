using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static string FormatToString<T>(this List<T> self, char separator, bool ignoreNull = true)
        {
            if (self is null) return null;

            if (self.Count == 0)
                return string.Empty;

            if (self.Count == 1)
            {
                if (self[0] == null)
                    return ignoreNull ? string.Empty : "NULL";
                return self[0].ToString();
            }

            int i = 1;

            //Pass over any nulls we cannot format
            if (ignoreNull)
            {
                while (i < self.Count && self[i] == null)
                    i++;

                if (i >= self.Count) //Entire list contains null values
                    return string.Empty;
            }

            StringBuilder sb = new StringBuilder(self[i].ToString());
            for (i++; i < self.Count; i++) //Value at i was already handled, so we must increase i at the start of for loop
            {
                sb.Append(' ')
                  .Append(separator)
                  .Append(' ');

                if (self[i] != null)
                    sb.Append(self[i].ToString().Trim());
                else if (!ignoreNull)
                    sb.Append("NULL");
            }
            return sb.ToString();
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
