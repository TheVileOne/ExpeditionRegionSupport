using System;
using System.Collections;
using System.Collections.Generic;

namespace LogUtils
{
    public class BufferedLinkedList<T> : IEnumerable<T> where T : class
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
            EnsureCapacity();

            LinkedListNode<T> node = nodeLeaser.Last;

            node.Value = value;
            InnerLinkedList.AddFirst(node);
            return node;
        }

        public LinkedListNode<T> AddLast(T value)
        {
            EnsureCapacity();

            LinkedListNode<T> node = nodeLeaser.Last;

            node.Value = value;
            InnerLinkedList.AddLast(node);
            return node;
        }

        public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)
        {
            EnsureCapacity();

            LinkedListNode<T> nodeBefore = nodeLeaser.Last;

            nodeBefore.Value = value;
            InnerLinkedList.AddBefore(node, nodeBefore);
            return nodeBefore;
        }

        public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value)
        {
            EnsureCapacity();

            LinkedListNode<T> nodeAfter = nodeLeaser.Last;

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

        public IEnumerator<T> GetEnumerator()
        {
            if (AllowModificationsDuringIteration)
                return new Enumerator(this);

            return InnerLinkedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (AllowModificationsDuringIteration)
                return new Enumerator(this);

            return InnerLinkedList.GetEnumerator();
        }

        public struct Enumerator : IEnumerator<T>
        {
            /// <summary>
            /// This is the node that controls the reference to the Current node
            /// </summary>
            private LinkedListNode<T> refNode;

            public LinkedListNode<T> CurrentNode
            {
                get
                {
                    return !started ? refNode : refNode.Next;
                }
            }

            public T Current => CurrentNode?.Value;

            object IEnumerator.Current => CurrentNode?.Value;

            private BufferedLinkedList<T> items;

            private bool started;

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
                if (items.Count == 0)
                {
                    started = false; //Enumeration cannot start on an empty list
                    return false;
                }

                if (!started)
                {
                    refNode = items.First;
                    started = true;
                    return true;
                }

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
                started = false;
                refNode = items.First;
            }
        }
    }
}
