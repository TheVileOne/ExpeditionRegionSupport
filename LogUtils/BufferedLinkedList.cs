using LogUtils.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils
{
    public class BufferedLinkedList<T> : ILinkedListEnumerable<T> where T : class
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

        public int Count => InnerLinkedList.Count;

        public LinkedListNode<T> First => InnerLinkedList.First;

        public LinkedListNode<T> Last => InnerLinkedList.Last;

        public bool AllowModificationsDuringIteration = true;

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

        public LinkedListNode<T> AddFirst(T value)
        {
            LinkedListNode<T> node = GetNodeFromLeaser();

            node.Value = value;
            InnerLinkedList.AddFirst(node);
            return node;
        }

        public LinkedListNode<T> AddLast(T value)
        {
            LinkedListNode<T> node = GetNodeFromLeaser();

            node.Value = value;
            InnerLinkedList.AddLast(node);
            return node;
        }

        public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)
        {
            LinkedListNode<T> nodeBefore = GetNodeFromLeaser();

            nodeBefore.Value = value;
            InnerLinkedList.AddBefore(node, nodeBefore);
            return nodeBefore;
        }

        public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value)
        {
            LinkedListNode<T> nodeAfter = GetNodeFromLeaser();

            nodeAfter.Value = value;
            InnerLinkedList.AddAfter(node, nodeAfter);
            return nodeAfter;
        }

        public bool Contains(T value)
        {
            return InnerLinkedList.Contains(value);
        }

        public LinkedListNode<T> Find(T value)
        {
            return InnerLinkedList.Find(value);
        }

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

        public bool Remove(T value)
        {
            FileUtils.WriteLine("test.txt", "Removing node by value");
            return InnerLinkedList.Remove(value);
        }

        public void Remove(LinkedListNode<T> node)
        {
            FileUtils.WriteLine("test.txt", "Removing node by reference");
            InnerLinkedList.Remove(node);
        }

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

        public ILinkedListEnumerable<T> Where(Func<T, bool> predicate)
        {
            FileUtils.WriteLine("test.txt", "Getting Where enumerable");

            if (AllowModificationsDuringIteration)
                return new WhereEnumerable(GetLinkedListEnumerator(), predicate);
            return new WhereEnumerableWrapper(Enumerable.Where(this, predicate));
        }

        public IEnumerator<T> GetEnumerator()
        {
            FileUtils.WriteLine("test.txt", "Getting enumerator");
            if (AllowModificationsDuringIteration)
                return new Enumerator(this);

            return new EnumeratorWrapper(InnerLinkedList.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ILinkedListEnumerator<T> GetLinkedListEnumerator()
        {
            return (ILinkedListEnumerator<T>)GetEnumerator();
        }

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

            public readonly T Current => CurrentNode?.Value;

            readonly object IEnumerator.Current => Current;

            private bool firstProcess = true;

            private readonly BufferedLinkedList<T> items;

            public Enumerator(BufferedLinkedList<T> list)
            {
                FileUtils.WriteLine("test.txt", "Enumerator created");
                items = list;
            }

            bool disposed = false;

            public void Dispose()
            {
                disposed = true;
                FileUtils.WriteLine("test.txt", "Disposing enumerator");
                Reset();
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                if (disposed)
                    FileUtils.WriteLine("test.txt", "Accessing a disposed enumerator");

                if (items == null)
                {
                    UtilityCore.BaseLogger.LogWarning("Enumerator items list should not be null");

                    firstProcess = false; //Enumeration cannot start on an empty list
                    return false;
                }

                //Move the enumerator by assigning a new reference node
                if (firstProcess)
                {
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

            public void Reset()
            {
                firstProcess = true;
                refNode = null;
            }
        }

        /// <summary>
        /// A simple wrapper for handling an IEnumerable similarly to an ILinkedListEnumerable despite not having the same functionality as one
        /// </summary>
        public struct EnumeratorWrapper : ILinkedListEnumerator<T>
        {
            private readonly IEnumerator<T> innerEnumerator;

            /// <summary>
            /// Not implemented by design - data is unavailable
            /// </summary>
            public readonly LinkedListNode<T> CurrentNode => throw new NotImplementedException();

            public readonly T Current => innerEnumerator.Current;

            readonly object IEnumerator.Current => Current;

            public EnumeratorWrapper(IEnumerator<T> enumerator)
            {
                innerEnumerator = enumerator;
            }

            public void Dispose()
            {
                //FileUtils.WriteLine("test.txt", "Disposing from enumerator wrapper");
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                return innerEnumerator.MoveNext();
            }

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

            public override IEnumerator<T> GetEnumerator()
            {
                return new EnumeratorWrapper(innerEnumerable.GetEnumerator());
            }
        }

        public class WhereEnumerable : ILinkedListEnumerable<T>
        {
            private ILinkedListEnumerator<T> enumerator;
            private Func<T, bool> predicate;

            internal WhereEnumerable()
            {
            }

            public WhereEnumerable(ILinkedListEnumerator<T> enumerator, Func<T, bool> predicate)
            {
                this.enumerator = enumerator;
                this.predicate = predicate;
            }

            public virtual IEnumerator<T> GetEnumerator()
            {
                return new WhereEnumerator(enumerator, predicate);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public ILinkedListEnumerator<T> GetLinkedListEnumerator()
            {
                return (ILinkedListEnumerator<T>)GetEnumerator();
            }
        }

        public struct WhereEnumerator : ILinkedListEnumerator<T>
        {
            private readonly ILinkedListEnumerator<T> innerEnumerator;
            private readonly Func<T, bool> predicate;

            public readonly LinkedListNode<T> CurrentNode => innerEnumerator.CurrentNode;

            public readonly T Current => innerEnumerator.Current;

            readonly object IEnumerator.Current => Current;

            public WhereEnumerator(ILinkedListEnumerator<T> enumerator, Func<T, bool> predicate)
            {
                FileUtils.WriteLine("test.txt", "Where enumerator created");

                this.innerEnumerator = enumerator;
                this.predicate = predicate;
            }

            public void Dispose()
            {
                //FileUtils.WriteLine("test.txt", "Disposing from where enumerator");
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
                };
                return predicateMatch;
            }

            public void Reset()
            {
                innerEnumerator.Reset();
            }
        }
    }

    public interface ILinkedListEnumerable<T> : IEnumerable<T> where T : class
    {
        public ILinkedListEnumerator<T> GetLinkedListEnumerator();
    }

    public interface ILinkedListEnumerator<T> : IEnumerator<T> where T : class
    {
        /// <summary>
        /// The LinkedListNode associated with Current
        /// </summary>
        public LinkedListNode<T> CurrentNode { get; }
    }
}
