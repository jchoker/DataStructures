using System;
using System.Collections;
using System.Collections.Generic;

namespace Choker.DataStructures
{
    /// <summary>
    /// An implementation of a hash-map (Dictionary in .Net context) that uses separate chaining technique as a collision resolution strategy,
    /// with a custom linkedlistnode used to represent a bucket in the table.
    /// </summary>
    /// <date>07.07.2020</date>
    /// <author>Jalal Choker, jalal.choker@gmail.com</author>
    public class HashMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : IEquatable<TKey>
    {
        const int DefaultCapacity = 3;
        const double DefaultLoadFactor = 0.75;

        public HashMap(int capacity) : this(capacity, DefaultLoadFactor) { }
        public HashMap(double maxLoadFactor) : this(DefaultCapacity, maxLoadFactor) { }

        // Designated constructor
        public HashMap(int capacity, double maxLoadFactor)
        {
            this.Capacity = capacity <= 0 ? throw new ArgumentOutOfRangeException(nameof(capacity)) : Math.Max(DefaultCapacity, capacity);
            this.MaxLoadFactor = (maxLoadFactor <= 0 || maxLoadFactor > 1) ? throw new ArgumentOutOfRangeException(nameof(maxLoadFactor)) : maxLoadFactor;
            this.Size = 0;
            this.table = new LinkedListNode[this.Capacity];
        }

        public int Size { get; private set; }
        public int Threshold => (int)(this.Capacity * this.MaxLoadFactor);
        public double MaxLoadFactor { get; }
        public int Capacity { get; private set; }
        public bool IsEmpty => this.Size == 0;

        LinkedListNode[] table;

        // Add/update a key/value in the hash-table
        public void Put(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key)); // By design hashtables don't allow null keys

            var loc = GetLocation(key);
            var buc = GetBucket(loc);

            if (buc == null)
            {
                table[loc] = new LinkedListNode(key, value); // first node in bucket
                IncrementSize();
            }
            else
            {
                LinkedListNode last = null;
                foreach (var node in buc)
                {
                    if (node.Key.Equals(key)) // key already exists then update value
                    {
                        node.Value = value;
                        return;
                    }
                    last = node;
                }
                // key doesn't exist in bucket
                {
                    var newEntry = new LinkedListNode(key, value);
                    // setup links
                    last.Next = newEntry;
                    newEntry.Previous = last;
                }
                IncrementSize();
            }
        }
        void IncrementSize()
        {
            if (++this.Size > this.Threshold) ResizeTable();
        }
        // Doubles the size of the internal table and re-maps entries
        void ResizeTable()
        {
            this.Capacity *= 2;
            var _table = new LinkedListNode[this.Capacity];

            foreach (var buc in this.table)
            {
                if (buc != null)
                {
                    foreach (var entry in buc)
                    {
                        var _loc = GetLocation(entry.Key);
                        var _buc = _table[_loc];
                        var _entry = new LinkedListNode(entry.Key, entry.Value);

                        if (_buc == null) _table[_loc] = _entry; // first node in bucket                        
                        else
                        {
                            LinkedListNode last = null;
                            foreach (var node in buc) last = node;
                            last.Next = _entry;
                            _entry.Previous = last;
                        }
                    }
                }
            }
            this.table = _table;
        }

        public TValue Get(TKey key)
        {
            var loc = GetLocation(key);
            var entry = SeekEntry(loc, key) ?? throw new KeyNotFoundException($"The given key '{key.ToString()}' was not present in the Hash-table.");
            return entry.Value;
        }

        public TValue this[TKey key] { get => Get(key); set => Put(key, value); } // indexer

        public bool TryGetValue(TKey key, out TValue value)
        {
            try
            {
                value = Get(key);
                return true;
            }
            catch (KeyNotFoundException)
            {
                value = default;
            }
            return false;
        }
        // returns a particular entry in a bucket if it exists otherwise null.
        LinkedListNode SeekEntry(int bucketIndex, TKey key)
        {
            var buc = this.table[bucketIndex];
            if (buc != null)
            {
                foreach (var entry in buc)
                    if (entry.Key.Equals(key)) // entry found
                        return entry;
            }
            return null; // not found
        }

        int GetLocation(TKey key) => key.GetHashCode() % this.Capacity; // uses the default hash function provided by the .Net framework, this can be changed

        LinkedListNode GetBucket(int bucketIndex) => this.table[bucketIndex];

        public bool ContainsKey(TKey key)
        {
            var loc = GetLocation(key);
            return SeekEntry(loc, key) != null;
        }

        // Removes a given key from the hash-table and return the value of exists otherwise default value.
        public TValue Remove(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var loc = GetLocation(key);
            var entry = SeekEntry(loc, key);
            if (entry == null) return default;

            // removal
            {
                var prv = entry.Previous;
                var nxt = entry.Next;

                if (prv != null)
                    prv.Next = nxt;

                if (nxt != null)
                    nxt.Previous = prv;

                if (prv == null) // when first node            
                    table[loc] = nxt;

                this.Size--;
            }

            return entry.Value;
        }

        public void Clear()
        {
            if (!IsEmpty)
            {
                //Array.Fill(table, null); // not available in .Net framework, available in .net core
                Array.Clear(table, 0, Capacity);
                this.Size = 0;
            }
        }

        public List<TKey> Keys
        {
            get
            {
                var keys = new List<TKey>();
                foreach (var kvp in this) keys.Add(kvp.Key);
                return keys;
            }
        }
        public List<TValue> Values
        {
            get
            {
                var values = new List<TValue>();
                foreach (var kvp in this) values.Add(kvp.Value);
                return values;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var buc in this.table)
                if (buc != null)
                    foreach (var entry in buc)
                        yield return new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region LinkedListNode
        /// <summary>
        /// An implementation of a doubly linkedlist node that associates value with key.
        /// </summary>
        private class LinkedListNode : IEnumerable<LinkedListNode>
        {
            public LinkedListNode(TKey key, TValue value) : this(key, value, null, null) { }
            // Designated constructor
            public LinkedListNode(TKey key, TValue value, LinkedListNode previous, LinkedListNode next)
            {
                this.Key = key;
                this.Value = value;
                this.Previous = previous;
                this.Next = next;
            }
            public TKey Key { get; } // readonly
            public TValue Value { get; set; }
            public LinkedListNode Previous { get; set; }
            public LinkedListNode Next { get; set; }

            #region IEnumerable
            public IEnumerator<LinkedListNode> GetEnumerator()
            {
                var node = this;
                while (node != null)
                {
                    yield return node;
                    node = node.Next;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            #endregion
        }
        #endregion
    }
}