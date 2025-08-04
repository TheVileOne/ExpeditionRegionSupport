using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils
{
    public class BufferedLinkedList<T> : ICollection<T>, ILinkedListEnumerable<T> where T : class
    {
        private readonly LinkedList<T> nodeLeaser;

        protected LinkedList<T> InnerLinkedList;

        private const int default_capacity = 5;

        private int _capacity;

        /// <summary>
        /// The amount of nodes managed by the node leaser
        /// </summary>
        public int Capacity
        {
            get => _capacity;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");

                int freeNodesAvailable = nodeLeaser.Count;

                if (value < Capacity)
                {
                    if (freeNodesAvailable <= Capacity - value)
                        throw new ArgumentException("Not enough free nodes available");

                    for (int i = 0; i < Capacity - value; i++)
                        nodeLeaser.RemoveLast(); //Remove nodes to match new capacity
                }
                else
                {
                    for (int i = 0; i < value - Capacity; i++)
                        nodeLeaser.AddLast(default(T));
                }
                _capacity = value;
            }
        }

        /// <inheritdoc/>
        public int Count => InnerLinkedList.Count;

        /// <inheritdoc cref="LinkedList{T}.First"/>
        public LinkedListNode<T> First => InnerLinkedList.First;

        /// <inheritdoc cref="LinkedList{T}.Last"/>
        public LinkedListNode<T> Last => InnerLinkedList.Last;

        /// <summary>
        /// Enable or disable collection modification protection
        /// </summary>
        /// <remarks>This is essentially deprecated</remarks>
        public bool AllowModificationsDuringIteration = true;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <summary>
        /// Construct a BufferedLinkedList object
        /// </summary>
        /// <param name="capacity">The amount of nodes to maintain in the leaser by default</param>
        public BufferedLinkedList(int capacity = default_capacity)
        {
            InnerLinkedList = new LinkedList<T>();
            nodeLeaser = new LinkedList<T>();

            Capacity = capacity;
        }

        /// <summary>
        /// Ensure that there are always nodes available in the node leaser
        /// </summary>
        internal void EnsureCapacity()
        {
            if (nodeLeaser.Count == 0) //Increase capacity when no free nodes are available to lease
                Capacity += default_capacity;
        }

        /// <inheritdoc/>
        public void Add(T item)
        {
            AddLast(item);
        }

        /// <inheritdoc cref="LinkedList{T}.AddFirst(T)"/>
        public LinkedListNode<T> AddFirst(T value)
        {
            LinkedListNode<T> node = GetNodeFromLeaser();

            node.Value = value;
            InnerLinkedList.AddFirst(node);
            return node;
        }

        /// <inheritdoc cref="LinkedList{T}.AddLast(T)"/>
        public LinkedListNode<T> AddLast(T value)
        {
            LinkedListNode<T> node = GetNodeFromLeaser();

            node.Value = value;
            InnerLinkedList.AddLast(node);
            return node;
        }

        /// <summary>
        /// Adds the specified value to a new node before the specified existing node
        /// </summary>
        /// <returns>The node storing the specified value</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)
        {
            LinkedListNode<T> nodeBefore = GetNodeFromLeaser();

            nodeBefore.Value = value;
            InnerLinkedList.AddBefore(node, nodeBefore);
            return nodeBefore;
        }

        /// <summary>
        /// Adds the specified value to a new node after the specified existing node
        /// </summary>
        /// <returns>The node storing the specified value</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value)
        {
            LinkedListNode<T> nodeAfter = GetNodeFromLeaser();

            nodeAfter.Value = value;
            InnerLinkedList.AddAfter(node, nodeAfter);
            return nodeAfter;
        }

        /// <inheritdoc/>
        public bool Contains(T value)
        {
            return InnerLinkedList.Contains(value);
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            InnerLinkedList.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc cref="LinkedList{T}.Find"/>
        public LinkedListNode<T> Find(T value)
        {
            return InnerLinkedList.Find(value);
        }

        /// <inheritdoc cref="LinkedList{T}.FindLast"/>
        public LinkedListNode<T> FindLast(T value)
        {
            return InnerLinkedList.FindLast(value);
        }

        internal LinkedListNode<T> GetNodeFromLeaser()
        {
            EnsureCapacity();

            LinkedListNode<T> node = nodeLeaser.Last;

            nodeLeaser.RemoveLast(); //Node needs to be removed from one LinkedList before it can be added to another
            return node;
        }

        /// <inheritdoc/>
        public bool Remove(T value)
        {
            //UtilityLogger.DebugLog("Removing node by value");
            return InnerLinkedList.Remove(value);
        }

        /// <inheritdoc cref="LinkedList{T}.Remove(LinkedListNode{T})"/>
        public void Remove(LinkedListNode<T> node)
        {
            //Handling an invalidated node will throw an exception if we don't return here.
            if (node.List == null) return;

            //UtilityLogger.DebugLog("Removing node by reference");
            InnerLinkedList.Remove(node);
        }

        /// <inheritdoc cref="LinkedList{T}.RemoveFirst"/>
        public void RemoveFirst()
        {
            LinkedListNode<T> node = InnerLinkedList.First;

            if (node != null)
            {
                node.Value = default;

                InnerLinkedList.RemoveFirst();
                nodeLeaser.AddLast(node); //This must be run after it is removed from the other LinkedList
            }
        }

        /// <inheritdoc cref="LinkedList{T}.RemoveLast"/>
        public void RemoveLast()
        {
            LinkedListNode<T> node = InnerLinkedList.Last;

            if (node != null)
            {
                node.Value = default;

                InnerLinkedList.RemoveLast();
                nodeLeaser.AddLast(node); //This must be run after it is removed from the other LinkedList
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            LinkedListNode<T> next, prev;

            next = InnerLinkedList.First;

            while (next != null)
            {
                prev = next;
                prev.Value = default;

                next = next.Next;

                InnerLinkedList.Remove(prev);
                nodeLeaser.AddLast(prev); //This must be run after it is removed from the other LinkedList
            }
        }

        /// <summary>
        /// Performs a Where query using the given predicate
        /// </summary>
        public ILinkedListEnumerable<T> Where(Func<T, bool> predicate)
        {
            if (AllowModificationsDuringIteration)
                return new WhereEnumerable(GetLinkedListEnumerator(), predicate);
            return new WhereEnumerableWrapper(Enumerable.Where(this, predicate));
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            if (AllowModificationsDuringIteration)
                return new Enumerator(this);

            return new EnumeratorWrapper(InnerLinkedList.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator specially designed to enumerate through this collection
        /// </summary>
        public ILinkedListEnumerator<T> GetLinkedListEnumerator()
        {
            return (ILinkedListEnumerator<T>)GetEnumerator();
        }

        /// <summary>
        /// The specialized enumerator for BufferedLinkedList
        /// </summary>
        public struct Enumerator : ILinkedListEnumerator<T>
        {
            /// <summary>
            /// This is the node that controls the reference to the Current node
            /// </summary>
            private LinkedListNode<T> refNode;

            /// <summary>
            /// The LinkedListNode associated with Current
            /// </summary>
            public readonly LinkedListNode<T> CurrentNode => firstProcess ? refNode : refNode?.Next;

            /// <inheritdoc/>
            public readonly T Current => CurrentNode?.Value;

            readonly object IEnumerator.Current => Current;

            private bool firstProcess = true;

            private readonly BufferedLinkedList<T> items;

            /// <summary>
            /// Constructs an Enumerator struct
            /// </summary>
            public Enumerator(BufferedLinkedList<T> list)
            {
                items = list ?? throw new ArgumentNullException(nameof(list));
            }

            /// <summary>
            /// Resets enumerator to a default state
            /// </summary>
            public void Dispose()
            {
                Reset();
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                //Move the enumerator by assigning a new reference node
                if (firstProcess)
                {
                    //checkForOverflowConditions();
                    if (refNode == null)
                    {
                        refNode = items.First;
                    }
                    else //Transition from first to second element needs only the flag changed, leaving refNode unchanged
                    {
                        firstProcess = false;
                    }
                }
                else if (refNode == null || refNode.Next == null)
                {
                    refNode = null;
                    return false;
                }
                else
                {
                    refNode = refNode.Next;
                }

                if (CurrentNode != null)
                    return true;

                firstProcess = false;
                refNode = null;
                return false;
            }

            /// <inheritdoc/>
            public void Reset()
            {
                firstProcess = true;
                refNode = null;
            }

            private void checkForOverflowConditions()
            {
                bool itemProcessOverflowed = false;
                int itemsAvailable = 0;
                var node = items.First;
                while (node != null)
                {
                    itemsAvailable++;
                    node = node.Next;

                    if (itemsAvailable > 10000)
                    {
                        itemProcessOverflowed = true;
                        break;
                    }
                }

                if (itemProcessOverflowed)
                    UtilityLogger.DebugLog("Item process overflow: Circular relationship likely");
            }
        }

        /// <summary>
        /// A simple wrapper for handling an IEnumerable similarly to an ILinkedListEnumerable despite not having the same functionality as one
        /// </summary>
        public readonly struct EnumeratorWrapper : ILinkedListEnumerator<T>
        {
            private readonly IEnumerator<T> innerEnumerator;

            /// <summary>
            /// Not implemented by design - data is unavailable
            /// </summary>
            public readonly LinkedListNode<T> CurrentNode => throw new NotImplementedException();

            /// <inheritdoc/>
            public readonly T Current => innerEnumerator.Current;

            readonly object IEnumerator.Current => Current;

            /// <summary>
            /// Constructs an EnumeratorWrapper struct
            /// </summary>
            /// <param name="enumerator">The enumerator that is intended to be wrapped</param>
            /// <exception cref="ArgumentNullException">Enumerator is null</exception>
            public EnumeratorWrapper(IEnumerator<T> enumerator)
            {
                innerEnumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
            }

            /// <summary>
            /// Resets enumerator to a default state
            /// </summary>
            public void Dispose()
            {
                Reset();
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                return innerEnumerator.MoveNext();
            }

            /// <inheritdoc/>
            public void Reset()
            {
                try
                {
                    innerEnumerator.Reset(); //This method might not be implemented
                }
                catch (NotImplementedException) { }
            }
        }

        public class WhereEnumerableWrapper : WhereEnumerable
        {
            private readonly IEnumerable<T> innerEnumerable;

            public WhereEnumerableWrapper(IEnumerable<T> enumerable)
            {
                innerEnumerable = enumerable;
            }

            /// <inheritdoc/>
            public override IEnumerator<T> GetEnumerator()
            {
                return new EnumeratorWrapper(innerEnumerable.GetEnumerator());
            }
        }

        public class WhereEnumerable : ILinkedListEnumerable<T>
        {
            private readonly ILinkedListEnumerator<T> enumerator;
            private readonly Func<T, bool> predicate;

            internal WhereEnumerable()
            {
            }

            public WhereEnumerable(ILinkedListEnumerator<T> enumerator, Func<T, bool> predicate)
            {
                this.enumerator = enumerator;
                this.predicate = predicate;
            }

            /// <inheritdoc/>
            public virtual IEnumerator<T> GetEnumerator()
            {
                return new WhereEnumerator(enumerator, predicate);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <inheritdoc/>
            public ILinkedListEnumerator<T> GetLinkedListEnumerator()
            {
                return (ILinkedListEnumerator<T>)GetEnumerator();
            }
        }

        public readonly struct WhereEnumerator : ILinkedListEnumerator<T>
        {
            private readonly ILinkedListEnumerator<T> innerEnumerator;
            private readonly Func<T, bool> predicate;

            /// <inheritdoc/>
            public readonly LinkedListNode<T> CurrentNode => innerEnumerator.CurrentNode;

            /// <inheritdoc/>
            public readonly T Current => innerEnumerator.Current;

            readonly object IEnumerator.Current => Current;

            public WhereEnumerator(ILinkedListEnumerator<T> enumerator, Func<T, bool> predicate)
            {
                this.innerEnumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
                this.predicate = predicate;
            }

            /// <summary>
            /// Resets enumerator to a default state
            /// </summary>
            public void Dispose()
            {
                Reset();
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                bool predicateMatch = false;
                while (!predicateMatch && innerEnumerator.MoveNext())
                {
                    predicateMatch = predicate(innerEnumerator.Current);
                }
                return predicateMatch;
            }

            /// <inheritdoc/>
            public void Reset()
            {
                innerEnumerator.Reset();
            }
        }
    }

    public interface ILinkedListEnumerable<T> : IEnumerable<T> where T : class
    {
        /// <summary>
        /// Returns an enumerator designed to iterate through a LinkedList
        /// </summary>
        ILinkedListEnumerator<T> GetLinkedListEnumerator();
    }

    public interface ILinkedListEnumerator<T> : IEnumerator<T> where T : class
    {
        /// <summary>
        /// The LinkedListNode associated with Current
        /// </summary>
        LinkedListNode<T> CurrentNode { get; }
    }
}
