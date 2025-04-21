using LogUtils.Helpers.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils
{
    public class WeakReferenceCollection<T> : IEnumerable<T> where T : class
    {
        protected ICollection<WeakReference<T>> InnerEnumerable;

        public WeakReferenceCollection() : this(new List<WeakReference<T>>())
        {
        }

        public WeakReferenceCollection(ICollection<WeakReference<T>> collectionBase)
        {
            InnerEnumerable = collectionBase;
        }

        public WeakReferenceCollection(IEnumerable<T> collection)
        {
            InnerEnumerable = collection.Select(item => new WeakReference<T>(item)).ToList();
        }

        public WeakReferenceCollection(IEnumerable<T> collection, ICollection<WeakReference<T>> collectionBase) : this(collectionBase)
        {
            collectionBase.AddRange(collection);
        }

        public IEnumerator<T> GetEnumerator()
        {
            var enumerator = InnerEnumerable.GetEnumerator();

            bool shouldCleanList = false;
            while (enumerator.MoveNext())
            {
                bool hasBeenCollected = !enumerator.Current.TryGetTarget(out T item);

                if (!hasBeenCollected)
                {
                    yield return item;
                    continue;
                }
                shouldCleanList = true;
            }

            if (shouldCleanList)
            {
                UtilityLogger.DebugLog("Cleaning entries");
                RemoveCollectedEntries();
            }
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public WeakReference<T> Add(T item)
        {
            return InnerEnumerable.Add(item);
        }

        public List<T> FindAll(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return this.Where(predicate).ToList();
        }

        public bool Remove(T item)
        {
            return InnerEnumerable.Remove(item);
        }

        public void RemoveAll(Predicate<T> predicate)
        {
            InnerEnumerable.RemoveWhere(predicate);
        }

        /// <summary>
        /// Remove entries that have been collected by the Garbage Collector
        /// </summary>
        public void RemoveCollectedEntries()
        {
            InnerEnumerable.RemoveCollectedEntries();
        }

        /// <summary>
        /// Returns the count of all entries in the collection, reference collected, or otherwise
        /// </summary>
        public int UnsafeCount()
        {
            return InnerEnumerable.Count;
        }
    }
}
