using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Choker.DataStructures
{
    /// <summary>
    /// A doubly linkedlist implementation
    /// </summary>
    /// <date>25.06.2020</date>
    /// <author>Jalal Choker, jalal.choker@gmail.com</author>
    public class LinkedList<T> : IEnumerable<T>
    {
        public LinkedList() { }

        // Designated constructor
        public LinkedList(T first)
        {
            if (first != null)
            {
                this.head = this.tail = new Node(first, this);
                Count++;
            }
        }

        Node head, tail;

        public int Count { get; private set; }
        public bool IsEmpty => this.Count == 0;

        public void Add(T data) => AddLast(data); // Adds an element to the tail of the linkedlist, O(1)

        public void AddLast(T data) // O(1)
        {
            if (IsEmpty) this.head = this.tail = new Node(data, this);
            else
            {
                this.tail.Next = new Node(data, previous: this.tail, next: null, this);
                this.tail = this.tail.Next;
            }
            Count++;
        }

        public void AddFirst(T data) // O(1)
        {
            if (IsEmpty) this.head = this.tail = new Node(data, this);
            else
            {
                this.head.Previous = new Node(data, previous: null, next: this.head, this);
                this.head = this.head.Previous;
            }
            Count++;
        }

        public void AddAt(int index, T data) // Adds an element at a specified index, O(n)
        {
            if (index < 0 || index > Count) throw new ArgumentOutOfRangeException(nameof(index));

            if (index == 0) AddFirst(data);
            else if (index == Count) AddLast(data);
            else
            {
                var tmp = this.head; // add after tmp
                for (int i = 0; i < index - 1; i++)
                {
                    tmp = tmp.Next;
                }
                var newNode = new Node(data, previous: tmp, next: tmp.Next, this);
                tmp.Next.Previous = newNode;
                tmp.Next = newNode;

                this.Count++;
            }
        }

        public T PeekFirst() // O(1)
        {
            if (IsEmpty) throw new InvalidOperationException("List is empty");
            return head.Data;
        }

        public T PeekLast() // O(1)
        {
            if (IsEmpty) throw new InvalidOperationException("List is empty");
            return tail.Data;
        }

        public T RemoveFirst() // O(1)
        {
            if (IsEmpty) throw new InvalidOperationException("List is empty");

            var data = head.Data; // extract data

            head = head.Next;
            Count--;

            if (IsEmpty) tail = null; else head.Previous = null;

            return data;
        }

        public T RemoveLast() // O(1)
        {
            if (IsEmpty) throw new InvalidOperationException("List is empty");

            var data = tail.Data;

            tail = tail.Previous;
            Count--;
            if (IsEmpty) head = null; else tail.Next = null;

            return data;
        }

        public T RemoveAt(int index) // O(n)
        {
            if (IsEmpty) throw new InvalidOperationException("List is empty");

            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));

            // micro-optimized traversal
            int i;
            Node trav;
            if (index < (Count / 2)) // search from the front of the list
            {
                for (i = 0, trav = this.head; i < index; i++)
                    trav = trav.Next;
            }
            else // search from the back
            {
                for (i = Count - 1, trav = this.tail; i > index; i--)
                    trav = trav.Previous;
            }

            return Remove(trav);
        }

        public bool Remove(T data) // Removes the first occurrence of a particular value from the list, O(n)
        {
            var rem = Find(data);

            if (rem == null)
                return false;
            else
            {
                Remove(rem);
                return true;
            };
        }

        T Remove(Node node) // O(1)
        {
            if (node.Previous == null) return RemoveFirst();
            if (node.Next == null) return RemoveLast();

            {
                // make the pointers of adjacent nodes skip over 'node'
                node.Previous.Next = node.Next;
                node.Next.Previous = node.Previous;
                this.Count--;

                var data = node.Data;
                node.Data = default;
                node = node.Previous = node.Next = null;

                return data;
            }
        }

        Node Find(T data) // O(n)
        {
            if (IsEmpty) return null;

            for (var trav = head; trav != null; trav = trav.Next)
            {
                if (trav.Data == null)
                {
                    if (data == null) return trav;
                }
                else
                {
                    if (trav.Data.Equals(data)) return trav;
                }
            }
            return null;
        }

        public int IndexOf(T value) // O(n)
        {
            int i = 0;

            for (var trav = this.head; trav != null; trav = trav.Next, i++)
            {
                if ((value == null && trav.Data == null) || (value != null && value.Equals(trav.Data))) // supports searching for null
                {
                    return i;
                }
            }

            return -1;
        }

        #region Contains
        public bool Contains(T value) => Find(value) != null;
        #endregion

        #region Clear
        public void Clear() // O(n)
        {
            var trav = this.head;
            while (trav != null) // Do we really need to set all links to null?
            {
                var nxt = trav.Next;

                trav.Previous = null;
                trav.Next = null;
                trav.Data = default;
                trav.List = null;

                trav = nxt;
            }

            this.head = this.tail = null;
            this.Count = 0;
        }
        #endregion

        #region IEnumerable
        public IEnumerator<T> GetEnumerator()
        {
            var trav = this.head;
            while (trav != null)
            {
                yield return trav.Data;
                trav = trav.Next;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        #endregion

        #region Overrides
        public override string ToString()
        {
            var sb = new StringBuilder("[ ");
            var trv = this.head;
            while (trv != null)
            {
                sb.Append(trv.Data);
                if (trv.Next != null) sb.Append(", ");
            }
            sb.Append(" ]");

            return sb.ToString();
        }
        #endregion

        #region Node
        class Node
        {
            public Node(T data) : this(data, null, null, null) { } // creates a stand-alone node

            public Node(T data, LinkedList<T> list) : this(data, null, null, list) { }

            public Node(T data, Node previous, Node next) : this(data, previous, next, null) { }

            // Designated constructor
            public Node(T data, Node previous, Node next, LinkedList<T> list)
            {
                this.Data = data;
                this.Previous = previous;
                this.Next = next;
                this.List = list;
            }

            public T Data { get; set; }
            public Node Previous { get; set; }
            public Node Next { get; set; }
            public LinkedList<T> List { get; set; }

            public override string ToString() => Data.ToString();
        }
        #endregion

        #region Copy
        public LinkedList<T> Copy() // a simple custom copy resulting in a deep copy
        {
            var copy = new LinkedList<T>();
            foreach (var item in this)
                copy.AddLast(item);
            return copy;
        }
        #endregion
    }
}