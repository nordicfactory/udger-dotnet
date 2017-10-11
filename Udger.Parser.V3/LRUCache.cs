﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Udger.Parser.V3
{
    /// <summary>
    ///     An LRU Cache implementation.
    /// 
    /// Copied from https://github.com/tejacques/LRUCache/blob/master/src/LRUCache/LRUCache.cs
    /// The original source was licensed under the Apache License 2.0.
    /// Its copied here instead of using the nuget since its a single file and the project hasn't been updated since its creation on Github.
    /// 
    /// This class is thread safe.
    /// </summary>
    /// <typeparam name="TK">The key type.</typeparam>
    /// <typeparam name="TV">The value type.</typeparam>
    public class LRUCache<TK, TV>
    {
        private readonly Dictionary<TK, CacheNode> _entries;
        private int _count;
        private CacheNode _head;
        private readonly bool _refreshEntries;
        private CacheNode _tail;
        private Timer _timer;
        private readonly TimeSpan _ttl;

        /// <summary>
        ///     A least recently used cache with a time to live.
        /// </summary>
        /// <param name="capacity">
        ///     The number of entries the cache will hold
        /// </param>
        /// <param name="hours">The number of hours in the TTL</param>
        /// <param name="minutes">The number of minutes in the TTL</param>
        /// <param name="seconds">The number of seconds in the TTL</param>
        /// <param name="refreshEntries">
        ///     Whether the TTL should be refreshed upon retrieval
        /// </param>
        public LRUCache(
            int capacity,
            int hours = 0,
            int minutes = 0,
            int seconds = 0,
            bool refreshEntries = true)
        {
            Capacity = capacity;
            _entries = new Dictionary<TK, CacheNode>(Capacity);
            _head = null;
            _tail = null;
            _count = 0;
            _ttl = new TimeSpan(hours, minutes, seconds);
            _refreshEntries = refreshEntries;
            if (_ttl > TimeSpan.Zero)
                _timer = new Timer(
                    Purge,
                    null,
                    (int)_ttl.TotalMilliseconds,
                    5000); // 5 seconds
        }

        /// <summary>
        ///     Gets the current number of entries in the cache.
        /// </summary>
        public int Count => _entries.Count;

        /// <summary>
        ///     Gets the maximum number of entries in the cache.
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        ///     Gets whether or not the cache is full.
        /// </summary>
        public bool IsFull => _count == Capacity;

        /// <summary>
        ///     Gets the item being stored.
        /// </summary>
        /// <returns>The cached value at the given key.</returns>
        public bool TryGetValue(TK key, out TV value)
        {
            value = default(TV);

            if (!_entries.TryGetValue(key, out var entry))
                return false;

            if (_refreshEntries)
                MoveToHead(entry);

            lock (entry)
            {
                value = entry.Value;
            }

            return true;
        }

        /// <summary>
        ///     Sets the item being stored to the supplied value.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to set in the cache.</param>
        public void Add(TK key, TV value)
        {
            TryAdd(key, value);
        }

        /// <summary>
        ///     Sets the item being stored to the supplied value.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to set in the cache.</param>
        /// <returns>True if the set was successful. False otherwise.</returns>
        public bool TryAdd(TK key, TV value)
        {
            if (!_entries.TryGetValue(key, out var entry))
                lock (this)
                {
                    if (!_entries.TryGetValue(key, out entry))
                    {
                        if (IsFull)
                        {
                            // Re-use the CacheNode entry
                            entry = _tail;
                            _entries.Remove(_tail.Key);

                            // Reset with new values
                            entry.Key = key;
                            entry.Value = value;
                            entry.LastAccessed = DateTime.UtcNow;

                            // Next and Prev don't need to be reset.
                            // Move to front will do the right thing.
                        }
                        else
                        {
                            _count++;
                            entry = new CacheNode
                            {
                                Key = key,
                                Value = value,
                                LastAccessed = DateTime.UtcNow
                            };
                        }
                        _entries.Add(key, entry);
                    }
                }
            else
                lock (entry)
                {
                    entry.Value = value;
                }

            MoveToHead(entry);

            // We don't need to lock here because two threads at this point
            // can both happily perform this check and set, since they are
            // both atomic.
            if (null == _tail)
                _tail = _head;

            return true;
        }

        /// <summary>
        ///     Removes the stored data.
        /// </summary>
        /// <returns>True if the removal was successful. False otherwise.</returns>
        public bool Clear()
        {
            lock (this)
            {
                _entries.Clear();
                _head = null;
                _tail = null;
                return true;
            }
        }

        /// <summary>
        ///     Moved the provided entry to the head of the list.
        /// </summary>
        /// <param name="entry">The CacheNode entry to move up.</param>
        private void MoveToHead(CacheNode entry)
        {
            if (entry == _head)
                return;

            // We need to lock here because we're modifying the entry
            // which is not thread safe by itself.
            lock (this)
            {
                RemoveFromLL(entry);
                AddToHead(entry);
            }
        }

        private void Purge(object state)
        {
            if (_ttl <= TimeSpan.Zero || _count == 0)
                return;

            lock (this)
            {
                var current = _tail;
                var now = DateTime.UtcNow;

                while (null != current
                       && now - current.LastAccessed > _ttl)
                {
                    Remove(current);
                    // Going backwards
                    current = current.Prev;
                }
            }
        }

        private void AddToHead(CacheNode entry)
        {
            entry.Prev = null;
            entry.Next = _head;

            if (null != _head)
                _head.Prev = entry;

            _head = entry;
        }

        private void RemoveFromLL(CacheNode entry)
        {
            var next = entry.Next;
            var prev = entry.Prev;

            if (null != next)
                next.Prev = entry.Prev;
            if (null != prev)
                prev.Next = entry.Next;

            if (_head == entry)
                _head = next;

            if (_tail == entry)
                _tail = prev;
        }

        private void Remove(CacheNode entry)
        {
            // Only to be called while locked from Purge
            RemoveFromLL(entry);
            _entries.Remove(entry.Key);
            _count--;
        }

        private class CacheNode
        {
            public CacheNode Next { get; set; }
            public CacheNode Prev { get; set; }
            public TK Key { get; set; }
            public TV Value { get; set; }
            public DateTime LastAccessed { get; set; }
        }
    }
}