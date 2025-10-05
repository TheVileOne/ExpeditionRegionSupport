using LogUtils.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LogUtils.Diagnostics.Tests
{
    /// <summary>
    /// Simple utility object for storing and running tests
    /// </summary>
    public class TestSuite : ICollection<ITestable>
    {
        /// <summary>
        /// This is set during the process when tests are being run through a TestSuite instance
        /// </summary>
        public static TestSuite ActiveSuite;

        protected const int DEFAULT_CAPACITY = 4;

        /// <summary>
        /// The handler to be used for all tests run through this test suite (null by default)
        /// </summary>
        public IConditionHandler Handler;

        protected ITestable[] InnerCollection;

        /// <summary>
        /// The size of the inner collection
        /// </summary>
        public int Capacity { get; protected set; }

        /// <summary>
        /// The current number of collection entries
        /// </summary>
        protected int Size;

        /// <inheritdoc/>
        public int Count => Size;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <summary>
        /// Constructs a new TestSuite instance
        /// </summary>
        public TestSuite()
        {
        }

        /// <summary>
        /// Constructs a new TestSuite instance with a specified default capacity
        /// </summary>
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
        /// <summary>
        /// Searches for all implementors of the ITestable interface in a given assembly, and adds an instance of the type to the test suite 
        /// </summary>
        public void AddTests(Assembly assembly)
        {
            bool errorLoadingTests = false;
            foreach (Type type in assembly.GetTypesSafely().Where(t => t.GetInterface(nameof(ITestable)) != null))
            {
                try
                {
                    Add((ITestable)Activator.CreateInstance(type));
                }
                catch
                {
                    errorLoadingTests = true;
                }
            }

            if (errorLoadingTests)
                UtilityLogger.LogWarning("Could not load one, or more tests");
        }

        /// <inheritdoc/>
        public void Add(ITestable item)
        {
            if (item == null)
                throw new ArgumentNullException("Collection does not support the storage of null values");

            UtilityLogger.Log("Loading " + item.Name);

            //Ensure we have enough space in the array for new values
            if (Capacity - Size == 0)
            {
                Capacity = Math.Max(Capacity + (Capacity / 2), DEFAULT_CAPACITY);
                Array.Resize(ref InnerCollection, Capacity);
            }

            InnerCollection[Size] = item;
            Size++;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            if (Size == 0) return;

            Array.Clear(InnerCollection, 0, Size);
        }

        /// <inheritdoc/>
        public bool Contains(ITestable item)
        {
            return Size > 0 && Array.FindIndex(InnerCollection, 0, 0, i => i == item) != -1;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
            ActiveSuite = this;
            try
            {
                var tests = GetEnumerator();

                while (tests.MoveNext())
                {
                    Condition.Result.ResetCount();
                    Run(tests.Current);
                }
            }
            finally
            {
                ActiveSuite = null;
                Condition.Result.ResetCount();
            }
        }

        /// <summary>
        /// Run tests that match a specific predicate
        /// </summary>
        public void RunTests(Func<ITestable, bool> predicate)
        {
            ActiveSuite = this;
            try
            {
                var tests = GetEnumerator();

                while (tests.MoveNext())
                {
                    if (predicate(tests.Current))
                    {
                        Condition.Result.ResetCount();
                        Run(tests.Current);
                    }
                }
            }
            finally
            {
                ActiveSuite = null;
                Condition.Result.ResetCount();
            }
        }

        public void Run(ITestable test)
        {
            Type type = test.GetType();

            //Get methods that have at least one custom attribute
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                       .Where(m => m.GetCustomAttributes(inherit: true).Any())
                                       .ToArray();

            IEnumerable<MethodInfo> preTestMethods = methods.Where(m => m.GetCustomAttribute(typeof(PreTestAttribute), inherit: true) != null),
                                    postTestMethods = methods.Where(m => m.GetCustomAttribute(typeof(PostTestAttribute), inherit: true) != null);

            foreach (var method in preTestMethods)
            {
                bool hasParams = method.GetParameters().Length > 0;

                if (!hasParams)
                {
                    method.Invoke(test, null);
                    continue;
                }

                UtilityLogger.LogError(new InvalidOperationException("Test condition must not have arguments"));
            }

            test.Test();

            foreach (var method in postTestMethods)
            {
                bool hasParams = method.GetParameters().Length > 0;

                if (!hasParams)
                {
                    method.Invoke(test, null);
                    continue;
                }

                UtilityLogger.LogError(new InvalidOperationException("Test condition must not have arguments"));
            }
        }
        #endregion
    }
}
