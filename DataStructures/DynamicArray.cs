using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Choker.DataStructures
{
    /// <summary>
    /// A generic dynamic array (ArrayList in Java, List in .Net) implementation
    /// </summary>
    /// <date>04.03.2020</date>
    /// <author>Jalal Choker, jalal.choker@gmail.com</author>
    public class DynamicArray<T> : IEnumerable<T>
    {
        // Initializes a new instance of the List class that is empty and has the default initial capacity of Zero.
        public DynamicArray() : this(0) { }

        /// <summary>
        /// Initializes a new instance of the List class that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        /// <Exceptions> T:System.ArgumentOutOfRangeException: capacity is less than 0.</Exceptions>
        public DynamicArray(int capacity)
        {
            this.capacity = capacity < 0 ? throw new ArgumentException(nameof(capacity), "Capacity must be positive") : capacity;
            this.list = new T[capacity];
        }

        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.List`1 class that
        /// contains elements copied from the specified collection and has sufficient capacity
        /// to accommodate the number of elements copied.
        /// </summary>
        public DynamicArray(IEnumerable<T> collection)
        {
            this.list = (collection == null) ? throw new ArgumentNullException(nameof(collection)) : collection.ToArray();
            this.capacity = this.list.Length;
            this.Count = this.capacity;
        }

        T[] list;
        int capacity;

        public int Count { get; private set; } = 0; // length user thinks array is
        public bool IsEmpty => this.Count == 0;

        // Gets or sets the element at the specified index.
        public T this[int index]
        {
            get
            {
                CheckIndexValid(index);
                return list[index];
            }
            set
            {
                CheckIndexValid(index);
                list[index] = value;
            }
        }

        void CheckIndexValid(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
        }

        // Adds an item to the end of the list. Amortized time is O(1).
        public void Add(T item)
        {
            if (Count == capacity) // time to resize
            {
                this.capacity = (capacity == 0) ? 1 : (this.capacity * 2);

                var copy = (T[])this.list.Clone();
                list = new T[capacity];

                for (int i = 0; i < Count; i++)
                    list[i] = copy[i];
            }

            list[Count++] = item;
        }

        // Removes an element at the specified index in this array, O(n).
        public T RemoveAt(int index)
        {
            CheckIndexValid(index);

            var data = list[index];
            var newList = new T[this.Count - 1]; // shrink array to remove trailing null paddings to avoid eventual empty list with enormous capacity

            for (int i = 0, j = 0; i < this.Count; i++, j++)
            {
                if (i == index) j--; // fix j for 1 time
                else
                {
                    newList[j] = list[i];
                }
            }

            this.list = newList;
            this.capacity = --Count;

            return data;
        }

        // removes 1st occurrence and returns true if found otherwise false
        public bool Remove(T item)
        {
            var ndx = IndexOf(item);
            if (ndx == -1) return false; // not found

            RemoveAt(ndx);
            return true;
        }

        // returns index of 1st occurrence if found otherwise -1
        public int IndexOf(T item)
        {
            for (int i = 0; i < this.Count; i++)
            {
                var elem = list[i];
                if ((item == null && elem == null) || (item != null && item.Equals(elem))) return i;
            }
            return -1; // not found
        }

        public bool Contains(T item) => IndexOf(item) != -1;

        // Removes all elements
        public void Clear()
        {
            for (int i = 0; i < Count; i++) list[i] = default;
            this.Count = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < this.Count; i++)
                yield return this.list[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString()
        {
            if (IsEmpty) return "[]";
            else
            {
                var sb = new StringBuilder("[");

                for (int i = 0; i < this.Count - 1; i++) // append all except last
                    sb.Append($"{list[i]}, ");

                sb.Append($"{list[this.Count - 1]}]"); // special treatment for last item

                return sb.ToString();
            }
        }
    }
}