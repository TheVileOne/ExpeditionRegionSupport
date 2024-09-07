using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils
{
    public class BufferedLinkedList<T> : ILinkedListEnumerable<T> where T : class
    {
        private LinkedList<T> nodeLeaser;

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
            return InnerLinkedList.Remove(value);
        }

        public void Remove(LinkedListNode<T> node)
        {
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
            if (AllowModificationsDuringIteration)
                return new WhereEnumerable(GetLinkedListEnumerator(), predicate);
            return new WhereEnumerableWrapper(Enumerable.Where(this, predicate));
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (AllowModificationsDuringIteration)
                return new Enumerator(this);
            return new EnumeratorWrapper(InnerLinkedList.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (AllowModificationsDuringIteration)
                return new Enumerator(this);
            return new EnumeratorWrapper(InnerLinkedList.GetEnumerator());
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
            public LinkedListNode<T> CurrentNode => firstProcess ? refNode : refNode?.Next;

            public T Current => CurrentNode?.Value;

            object IEnumerator.Current => CurrentNode?.Value;

            private BufferedLinkedList<T> items;

            private bool firstProcess;

            public Enumerator(BufferedLinkedList<T> list)
            {
                items = list;
            }

            public IEnumerable<T> EnumerateAll()
            {
                Reset();

                while (MoveNext())
                    yield return Current;
                yield break;
            }

            public void Dispose()
            {
                refNode = null;
                items = null;
            }

            public bool MoveNext()
            {
                if (items == null)
                {
                    UtilityCore.BaseLogger.LogWarning("Enumerator items list should not be null");

                    firstProcess = false; //Enumeration cannot start on an empty list
                    return false;
                }

                if (items.Count == 0)
                {
                    firstProcess = false; //Enumeration cannot start on an empty list
                    return false;
                }

                if (refNode == null)
                {
                    refNode = items.First;
                    firstProcess = true;
                    return true;
                }

                firstProcess = false;

                var next = refNode.Next;

                //The reference node is only changed when list can be advanced
                if (next != null && next != items.First)
                {
                    refNode = next;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                firstProcess = false;
                refNode = null;
            }
        }

        public readonly struct EnumeratorWrapper : ILinkedListEnumerator<T>
        {
            private readonly IEnumerator<T> innerEnumerator;

            public LinkedListNode<T> CurrentNode => throw new NotImplementedException();

            public T Current => innerEnumerator.Current;

            object IEnumerator.Current => Current;

            public EnumeratorWrapper(IEnumerator<T> enumerator)
            {
                innerEnumerator = enumerator;
            }

            public void Dispose()
            {
                innerEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                return innerEnumerator.MoveNext();
            }

            public void Reset()
            {
                innerEnumerator.Reset();
            }

            public IEnumerable<T> EnumerateAll()
            {
                try
                {
                    Reset(); //This method might not be implemented
                }
                catch { };

                while (MoveNext())
                    yield return Current;
                yield break;
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
                return new WhereEnumerator(enumerator, predicate);
            }

            public ILinkedListEnumerator<T> GetLinkedListEnumerator()
            {
                return (ILinkedListEnumerator<T>)GetEnumerator();
            }
        }

        public readonly struct WhereEnumerator : ILinkedListEnumerator<T>
        {
            private readonly ILinkedListEnumerator<T> innerEnumerator;
            private readonly Func<T, bool> predicate;

            public LinkedListNode<T> CurrentNode => innerEnumerator.CurrentNode;

            public T Current => innerEnumerator.Current;

            object IEnumerator.Current => Current;

            public WhereEnumerator(ILinkedListEnumerator<T> enumerator, Func<T, bool> predicate)
            {
                this.innerEnumerator = enumerator;
                this.predicate = predicate;
            }

            public void Dispose()
            {
                innerEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                bool predicateMatch = false;
                while (innerEnumerator.MoveNext() && !predicateMatch)
                {
                    predicateMatch = predicate(innerEnumerator.Current);
                };
                return predicateMatch;
            }

            public void Reset()
            {
                innerEnumerator.Reset();
            }

            public IEnumerable<T> EnumerateAll()
            {
                Reset();

                while (MoveNext())
                    yield return Current;
                yield break;
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
        public IEnumerable<T> EnumerateAll();
    }
}
