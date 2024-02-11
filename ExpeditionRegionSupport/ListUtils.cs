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
    }
}
