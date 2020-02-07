using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace PrecisionCache
{
    public sealed class SimpleLocalCache : IPrecisionCache, IDisposable
    {
        private class SimpleCacheItem
        {
            internal object Value;
            internal DateTime ExpirationTime;
            internal string[] Tags;
        }

        private readonly Timer _trimTimer;
        private readonly ConcurrentDictionary<string, SimpleCacheItem> _localCacheItems = new ConcurrentDictionary<string, SimpleCacheItem>();
        private readonly int _timeout;

        public int Count => _localCacheItems.Count;

        /// <summary>
        /// Use a trimming interval when you're caching large amounts of data
        /// that most likely won't be reused once expired.
        /// </summary>
        /// <param name="itemTimeout">lifetime of cached object</param>
        /// <param name="cacheTrimmingInterval">How often expired objects should be removed, in minutes</param>
        public SimpleLocalCache(int itemTimeout = 30, int cacheTrimmingInterval = 0)
        {
            _timeout = itemTimeout;
            if (cacheTrimmingInterval == 0) // default of 0 means no trimming
                return;

            var trimmingInterval = new TimeSpan(0, cacheTrimmingInterval, 0);
            _trimTimer = new Timer(TrimExpiredItems, null, trimmingInterval, trimmingInterval);
        }

        public void Dispose()
        {
            _trimTimer.Dispose();
            Clear();
        }

        /// <summary>
        /// Find a cached object
        /// </summary>
        /// <param name="key">lookup key</param>
        /// <param name="value">It is up to the caller to keep track of the object type</param>
        /// <returns>success/failure</returns>
        public bool TryGetValue(string key, out object value)
        {
            if (_localCacheItems.TryGetValue(key, out var cacheItem))
            {
                if (cacheItem.ExpirationTime > DateTime.Now)
                {
                    value = cacheItem.Value;
                    return true;
                }
                TryRemove(key);
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Add object to the cache or update existing value
        /// </summary>
        /// <param name="key">lookup key</param>
        /// <param name="value">the caller knows the object type</param>
        /// <param name="tags">can be used for assigning tags to cache items</param>
        public void AddOrUpdate(string key, object value, params string[] tags)
        {
            _localCacheItems[key] = new SimpleCacheItem() { ExpirationTime = DateTime.Now.AddMinutes(_timeout), Value = value, Tags = tags };
        }

        /// <summary>
        /// Removes an object from the cache if it exists
        /// </summary>
        /// <param name="key"></param>
        public void TryRemove(string key)
        {
            _localCacheItems.TryRemove(key, out var _);
        }

        /// <summary>
        /// Clears cache
        /// </summary>
        public void Clear()
        {
            _localCacheItems.Clear();
        }

        /// <summary>
        /// Count of items with a specific tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public int CountTag(string tag)
        {
            var count = _localCacheItems.Count(item => item.Value.Tags.Contains(tag));

            return count;
        }

        /// <summary>
        /// Deletes all items with a specific tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public int FlushTag(string tag)
        {
            var count = 0;
            var deletionKeys = _localCacheItems.Where(item => item.Value.Tags.Contains(tag)).Select(item => item.Key).ToList();
            foreach (var key in deletionKeys)
            {
                TryRemove(key);
                count++;
            }
            return count;
        }

        /// <summary>
        /// Called from a timer event (see the constructor) for periodic removal of expired items
        /// </summary>
        /// <param name="unused"></param>
        private void TrimExpiredItems(object unused)
        {
            var now = DateTime.Now;
            var deletionKeys = _localCacheItems.Where(item => now > item.Value.ExpirationTime).Select(item => item.Key).ToList();

            foreach (var key in deletionKeys)
                TryRemove(key);
        }
    }
}
