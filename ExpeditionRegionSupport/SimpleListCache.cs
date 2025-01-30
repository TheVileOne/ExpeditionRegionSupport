using System;
using System.Collections.Generic;

namespace ExpeditionRegionSupport
{
    public class SimpleListCache<T>
    {
        /// <summary>
        /// A unique identifier that can be used to distinguish between two cache instances
        /// </summary>
        protected int CacheID = -1;

        protected List<T> _items = new List<T>();

        /// <summary>
        /// A collection of stored items
        /// </summary>
        public List<T> Items
        {
            get => _items;
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                //This reference should not be overwritten. Transfer values to existing reference instead.
                _items.Clear();
                Store(value);
            }
        }

        public SimpleListCache()
        {
            AssignUniqueID();
        }

        /// <summary>
        /// Stores an item in the item cache
        /// </summary>
        public virtual T Store(T item)
        {
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Stores item in the item cache from a provided IEnumerable
        /// </summary>
        public virtual void Store(IEnumerable<T> items)
        {
            _items.AddRange(items);
        }

        protected virtual void AssignUniqueID()
        {
        }
    }
}
