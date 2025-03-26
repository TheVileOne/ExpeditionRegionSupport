using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils
{
    public class WeakReferenceCollection<T> : IEnumerable<T> where T : class
    {
        protected List<WeakReference<T>> InnerList = new List<WeakReference<T>>();

        public IEnumerator<T> GetEnumerator()
        {
            var enumerator = InnerList.GetEnumerator();

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
                RemoveCollectedEntries();
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public WeakReference<T> Add(T item)
        {
            WeakReference<T> reference = new WeakReference<T>(item);
            InnerList.Add(reference);
            return reference;
        }

        public List<T> FindAll(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return this.Where(predicate).ToList();
        }

        public bool Remove(T item)
        {
            for (int i = 0; i < InnerList.Count; i++)
            {
                var reference = InnerList[i];

                if (reference.TryGetTarget(out T _item) && _item == item)
                {
                    InnerList.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public void RemoveAll(Func<T, bool> predicate)
        {
            for (int i = 0; i < InnerList.Count; i++)
            {
                var reference = InnerList[i];

                if (reference.TryGetTarget(out T _item) && predicate(_item))
                {
                    InnerList.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Remove entries that have been collected by the Garbage Collector
        /// </summary>
        public void RemoveCollectedEntries()
        {
            InnerList.RemoveAll(reference => !reference.TryGetTarget(out _));
        }
    }
}
