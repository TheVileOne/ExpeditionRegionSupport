using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Helpers.Extensions
{
    public static partial class ExtensionMethods
    {
        public static WeakReference<T> Add<T>(this ICollection<WeakReference<T>> collection, T item) where T : class
        {
            WeakReference<T> reference = new WeakReference<T>(item);
            collection.Add(reference);
            return reference;
        }

        public static void AddRange<T>(this ICollection<WeakReference<T>> collection, IEnumerable<T> items) where T : class
        {
            foreach (var item in items)
                collection.Add(item);
        }

        public static bool Remove<T>(this ICollection<WeakReference<T>> collection, T item) where T : class
        {
            var list = collection as IList<WeakReference<T>>;

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var reference = list[i];

                    if (reference.TryGetTarget(out T _item) && _item == item)
                    {
                        list.RemoveAt(i);
                        return true;
                    }
                }
            }
            else
            {
                var reference = Enumerable.FirstOrDefault(collection, i =>
                {
                    if (i.TryGetTarget(out T _item))
                        return _item == item;
                    return false;
                });

                if (reference != null)
                    return collection.Remove(reference);
            }
            return false;
        }

        public static void RemoveWhere<T>(this ICollection<WeakReference<T>> collection, Predicate<T> predicate) where T : class
        {
            var list = collection as IList<WeakReference<T>>;

            int entryCount = collection.Count;
            int index = 0;
            if (list != null)
            {
                while (index < entryCount)
                {
                    var reference = list[index];

                    if (reference.TryGetTarget(out T _item) && predicate(_item))
                    {
                        list.RemoveAt(index);
                        entryCount--;
                        continue;
                    }
                    index++;
                }
            }
            else
            {
                while (index < entryCount)
                {
                    var reference = collection.ElementAt(index);

                    if (reference.TryGetTarget(out T _item) && predicate(_item))
                    {
                        collection.Remove(reference);
                        entryCount--;
                        continue;
                    }
                    index++;
                }
            }
        }

        /// <summary>
        /// Remove entries that have been garbage collected
        /// </summary>
        /// <returns>The number of removed entries</returns>
        public static int RemoveCollectedEntries<T>(this ICollection<WeakReference<T>> collection) where T : class
        {
            var list = collection as IList<WeakReference<T>>;

            int collectedCount = 0;

            int entryCount = collection.Count;
            int index = 0;
            if (list != null)
            {
                while (index < entryCount)
                {
                    var reference = list[index];

                    if (!reference.TryGetTarget(out _))
                    {
                        list.RemoveAt(index);

                        collectedCount++;
                        entryCount--;
                        continue;
                    }
                    index++;
                }
            }
            else
            {
                while (index < entryCount)
                {
                    var reference = collection.ElementAt(index);

                    if (!reference.TryGetTarget(out _))
                    {
                        collection.Remove(reference);

                        collectedCount++;
                        entryCount--;
                        continue;
                    }
                    index++;
                }
            }
            return collectedCount;
        }
    }
}
