using System;
using System.Collections;
using System.Collections.Generic;

namespace LogUtils.Diagnostics.Tests
{
    /// <summary>
    /// Simple utility object for storing and running tests
    /// </summary>
    public class TestSuite : ICollection<ITestable>
    {
        protected const int DEFAULT_CAPACITY = 4;

        protected ITestable[] InnerCollection;

        /// <summary>
        /// The size of the inner collection
        /// </summary>
        public int Capacity { get; protected set; }

        /// <summary>
        /// The current number of collection entries
        /// </summary>
        protected int Size;

        public int Count => Size;

        public bool IsReadOnly => false;

        public TestSuite()
        {
            List<int> list = new List<int>();
            list.Capacity = DEFAULT_CAPACITY;
        }

        public TestSuite(int capacity)
        {
            EnsureCapacity(capacity);
        }

        public ITestable this[int index]
        {
            get
            {
                if (index < 0 || index >= Size)
                    throw new IndexOutOfRangeException("Index must be within the size of the collection");
                return InnerCollection[index];
            }

            set
            {
                if (index < 0 || index >= Size)
                    throw new IndexOutOfRangeException("Index must be within the size of the collection");
                InnerCollection[index] = value;
            }
        }

        #region Collection code
        public void Add(ITestable item)
        {
            if (item == null)
                throw new ArgumentNullException("Collection does not support the storage of null values");

            //Ensure we have enough space in the array for new values
            if (Capacity - Size == 0)
            {
                Capacity = Math.Max(Capacity + (Capacity / 2), DEFAULT_CAPACITY);
                Array.Resize(ref InnerCollection, Capacity);
            }

            InnerCollection[Size] = item;
            Size++;
        }

        public void Clear()
        {
            if (Size == 0) return;

            Array.Clear(InnerCollection, 0, Size);
        }

        public bool Contains(ITestable item)
        {
            return Size > 0 && Array.FindIndex(InnerCollection, 0, 0, i => i == item) != -1;
        }

        public void CopyTo(ITestable[] array, int arrayIndex)
        {
            if (Size == 0) return;

            Array.Copy(InnerCollection, 0, array, arrayIndex, Size);
        }

        public void EnsureCapacity(int capacity)
        {
            //Capacity is already at a desired level 
            if (capacity <= Capacity)
                return;

            Array.Resize(ref InnerCollection, capacity);
            Capacity = capacity;
        }

        public IEnumerator<ITestable> GetEnumerator()
        {
            if (Size == 0)
                yield return null;

            for (int i = 0; i < Size; i++)
                yield return InnerCollection[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(ITestable item)
        {
            if (item == null) return false;

            bool itemFound = false;
            for (int i = 0; i < Size; i++)
            {
                if (itemFound)
                {
                    //Move all items to the left by one index
                    InnerCollection[i - 1] = InnerCollection[i];
                    InnerCollection[i] = null;
                }
                else if (item == InnerCollection[i])
                {
                    InnerCollection[i] = null;
                    Size--;
                    itemFound = true;
                }
            }
            return itemFound;
        }

        /// <summary>
        /// Sorts the elements in a range of elements in an System.Array using the specified IComparer generic interface
        /// </summary>
        /// <param name="index">The starting index of the range to sort</param>
        /// <param name="count">The amount of elements in the range to sort</param>
        /// <param name="comparer">The IComparer to use</param>
        /// <exception cref="ArgumentNullException">Provided comparer is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Index is less than the lower bound of array. -or- count is less than zero.</exception>
        public void Sort(int index, int count, IComparer<ITestable> comparer)
        {
            if (Size == 0) return;

            if (comparer == null)
                throw new ArgumentNullException("Comparer cannot be null");

            Array.Sort(InnerCollection, index, count, comparer);
        }
        #endregion
        #region Application code
        public void RunAllTests()
        {
            var enumerator = GetEnumerator();

            while (enumerator.MoveNext())
            {
                var currentTest = enumerator.Current;
                currentTest.Test();
            }
        }
        #endregion
    }
}
