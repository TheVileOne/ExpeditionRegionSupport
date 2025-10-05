using System.Collections;
using System.Collections.Generic;

namespace LogUtils.Collections
{
    /// <summary>
    /// A hashset wrapper class with built-in ReadOnly support from a bindable source
    /// </summary>
    /// <remarks>Class is mainly designed to keep user changes to LogProperties safe from modification </remarks>
    public class ValueCollection<T> : ISet<T>
    {
        private ReadOnlyProvider _readOnlySource;

        /// <inheritdoc/>
        public int Count => Values.Count;

        /// <inheritdoc/>
        public virtual bool IsReadOnly => _readOnlySource != null && _readOnlySource.Invoke();

        /// <summary>
        /// The underlying dataset
        /// </summary>
        protected ISet<T> Values;

        /// <summary>
        /// Creates a new <see cref="ValueCollection{T}"/> instance
        /// </summary>
        /// <param name="readOnlySource">The binding source for determining the ReadOnly state of the collection</param>
        public ValueCollection(ReadOnlyProvider readOnlySource = null)
        {
            _readOnlySource = readOnlySource;
            Reset();
        }

        /// <inheritdoc/>
        public virtual bool Add(T item)
        {
            return !IsReadOnly && Values.Add(item);
        }

        void ICollection<T>.Add(T item)
        {
            ((ICollection<T>)Values).Add(item);
        }

        void ICollection<T>.Clear()
        {
            Values.Clear();
        }

        /// <inheritdoc/>
        public virtual bool Remove(T item)
        {
            return !Values.IsReadOnly && Values.Remove(item);
        }

        /// <inheritdoc/>
        public bool Contains(T item)
        {
            return Values.Contains(item);
        }
        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            Values.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Changes collection back to its initial state
        /// </summary>
        protected virtual void Reset()
        {
            if (Values != null)
            {
                Values.Clear();
                return;
            }
            Values = new HashSet<T>();
        }

        #region ISet implementation
        /// <inheritdoc/>
        public void UnionWith(IEnumerable<T> other)
        {
            Values.UnionWith(other);
        }

        /// <inheritdoc/>
        public void IntersectWith(IEnumerable<T> other)
        {
            Values.IntersectWith(other);
        }

        /// <inheritdoc/>
        public void ExceptWith(IEnumerable<T> other)
        {
            Values.ExceptWith(other);
        }

        /// <inheritdoc/>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            Values.SymmetricExceptWith(other);
        }

        /// <inheritdoc/>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return Values.IsSubsetOf(other);
        }

        /// <inheritdoc/>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return Values.IsSupersetOf(other);
        }

        /// <inheritdoc/>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return Values.IsProperSupersetOf(other);
        }

        /// <inheritdoc/>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return Values.IsProperSubsetOf(other);
        }

        /// <inheritdoc/>
        public bool Overlaps(IEnumerable<T> other)
        {
            return Values.Overlaps(other);
        }

        /// <inheritdoc/>
        public bool SetEquals(IEnumerable<T> other)
        {
            return Values.SetEquals(other);
        }
        #endregion

        /// <inheritdoc/>
        public virtual IEnumerator<T> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public delegate bool ReadOnlyProvider();
}
