﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using IsraelHiking.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IsraelHiking.API.Services
{
    /// <summary>
    /// Least recently used cache - vary basic implementation.
    /// </summary>
    /// <typeparam name="TKey">Key's type</typeparam>
    /// <typeparam name="TValue">Value's type</typeparam>
    public class LruCache<TKey, TValue> where TKey: class
    {
        private readonly ILogger _logger;
        private readonly ConfigurationData _options;
        private readonly ConcurrentDictionary<TKey, CacheItem> _dictionary = new ConcurrentDictionary<TKey, CacheItem>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public LruCache(IOptions<ConfigurationData> options, ILogger logger)
        {
            _logger = logger;
            _options = options.Value;
            _logger.LogInformation("Initializing LRU cache.");
        }

        private class CacheItem
        {
            public TValue Value { get; }

            public DateTime LastUsed { get; set; }

            public CacheItem(TValue value)
            {
                Value = value;
                LastUsed = DateTime.Now;
            }
        }

        /// <summary>
        /// Add item the the cache
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        public void Add(TKey key, TValue value)
        {
            _dictionary[key] = new CacheItem(value);
            while (_dictionary.Keys.Count > _options.MaxCacheSize)
            {
                _logger.LogInformation("LRU cache is full - removing least recently used item.");
                var pair = _dictionary.OrderBy(v => v.Value.LastUsed).First();
                _dictionary.TryRemove(pair.Key, out var _);
            }
        }

        /// <summary>
        /// Get item from the cache
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The item</returns>
        public TValue Get(TKey key)
        {
            if (key == null || _dictionary.ContainsKey(key) == false)
            {
                return default(TValue);
            }
            _dictionary[key].LastUsed = DateTime.Now;
            return _dictionary[key].Value;
        }

        /// <summary>
        /// Get the first value that matches the key, assuming this case is one-to-one mostly.
        /// </summary>
        /// <param name="value">The value to look for</param>
        /// <returns>The key</returns>
        public TKey ReverseGet(TValue value)
        {
            return _dictionary.FirstOrDefault(v => v.Value.Value.Equals(value)).Key;
        }
    }
}
