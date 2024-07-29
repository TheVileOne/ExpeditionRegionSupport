﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace LogUtils
{
    public class BufferedLinkedList<T> : IEnumerable<T>
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
            return InnerLinkedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return InnerLinkedList.GetEnumerator();
        }
    }
}