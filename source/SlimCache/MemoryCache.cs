using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace SlimCache
{
    /// <summary>
    /// Class supports in memory cache for Silverlight applications. Silverlight 2 and 3 are supported.
    /// Each minute the cache items are iterated for validility, invalid cache items are removed.
    /// </summary>
    public class MemoryCache : ICache, IDisposable
    {
        private readonly IDictionary<string, CacheItem> _cacheItems = new Dictionary<string, CacheItem>();

        private readonly TimeSpan _period = TimeSpan.FromMinutes(1);
        private readonly TimeSpan _startTimeSpan = TimeSpan.Zero;
        private readonly TimeSpan _stopTimeSpan = TimeSpan.FromMilliseconds(-1);
        private readonly Timer _timer;
        private TimerState _state = TimerState.Stopped;

        public MemoryCache(Action<string> expirationCallback)
        {
            _timer = new Timer(CleanUpItems, this, _stopTimeSpan, _period);
        }

        /// <summary>
        /// All the CacheItems are in a Dictionary
        /// </summary>
        public IDictionary<string, CacheItem> CacheItems
        {
            get { return _cacheItems; }
        }

        /// <summary>
        /// Get full CacheItem based on key.
        /// </summary>
        /// <param name="key">The key for which a CacheItem is stored.</param>
        /// <returns>The CacheItem stored for the given key, if it is still valid. Otherwise null.</returns>
        public CacheItem this[string key]
        {
            get
            {
                if (CacheItems.ContainsKey(key))
                {
                    CacheItem ci = CacheItems[key];
                    if (ci.IsValid())
                        return CacheItems[key];
                }
                return null;
            }
            set { CacheItems[key] = value; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _timer.Dispose();
        }

        #endregion

        /// <summary>
        /// Get data typed directly for given key.
        /// </summary>
        /// <typeparam name="T">The Type for which the data is set. If the type is wrong null will be returned.</typeparam>
        /// <param name="key">The key for which a CacheItem is stored.</param>
        /// <returns>The data typed for the given key, if it is still valid. Otherwise null.</returns>
        public T Get<T>(string key) where T : class
        {
            CacheItem item = this[key];
            if (item != null)
                return item.GetData<T>();
            return null;
        }

        /// <summary>
        /// Get data untyped directly for given key.
        /// </summary>
        /// <param name="key">The key for which a CacheItem is stored.</param>
        /// <returns>The data untyped for the given key, if it is still valid. Otherwise null.</returns>
        public object Get(string key)
        {
            CacheItem item = this[key];
            if (item != null)
                return item.GetData();
            return null;
        }

        private void StartTimer()
        {
            if (_state == TimerState.Stopped)
            {
                _timer.Change(_startTimeSpan, _period);
                _state = TimerState.Started;
            }
        }

        private void StopTimer()
        {
            if (_state == TimerState.Started)
            {
                _timer.Change(_stopTimeSpan, _period);
                _state = TimerState.Stopped;
            }
        }

        /// <summary>
        /// Clean up items that are not longer valid.
        /// </summary>
        /// <param name="state">Expect state to be the cache object.</param>
        private static void CleanUpItems(object state)
        {
            var cache = state as MemoryCache;
            if (cache != null)
            {
                List<KeyValuePair<string, CacheItem>> itemsToRemove =
                    cache.CacheItems.Where(i => !i.Value.IsValid()).ToList();
                foreach (var item in itemsToRemove)
                {
                    cache.CacheItems.Remove(item.Key);
                }
                if (cache.CacheItems.Count == 0)
                    cache.StopTimer();
            }
        }

        /// <summary>
        /// Add a new item to the cache. If the key is already used it will be overwritten.
        /// </summary>
        /// <param name="key">The key for which a CacheItem is stored.</param>
        /// <param name="value"></param>
        public void Add<T>(T entry, string key, DateTime absoluteExpiration) where T : class
        {
            var value = new CacheItem(entry, absoluteExpiration);
            if (_cacheItems.ContainsKey(key))
                _cacheItems.Remove(key);
            _cacheItems.Add(key, value);
            StartTimer();
        }

        /// <summary>
        /// Removes the item for the given key from the cache.
        /// </summary>
        /// <param name="key">The key for which a CacheItem is stored.</param>
        /// <returns></returns>
        public void Remove(string key)
        {
            _cacheItems.Remove(key);
            if (_cacheItems.Count == 0)
                StopTimer();
        }

        #region Nested type: TimerState

        /// <summary>
        /// Used for determing TimerState
        /// </summary>
        private enum TimerState
        {
            Stopped,
            Started
        }

        #endregion

        public void Empty()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> Keys
        {
            get { return _cacheItems.Keys; }
        }

        public bool Exists(string key)
        {
            return _cacheItems.ContainsKey(key);
        }
    }

    /// <summary>
    /// Defines an item that's stored in the Cache.
    /// </summary>
    public class CacheItem
    {
        private readonly DateTime _creationDate = DateTime.Now;

        private readonly DateTime _expiration;

        /// <summary>
        /// Constructs a cache item with for the data with a validity of validDuration.
        /// </summary>
        /// <param name="data">The data for the cache.</param>
        /// <param name="validDuration">The duration for the data being valid in the cache.</param>
        public CacheItem(object data, DateTime expiration)
        {
            _expiration = expiration;
            Data = data;
        }

        /// <summary>
        /// The data in the Cache.
        /// </summary>
        public object Data { set; private get; }

        /// <summary>
        /// Gets the Data typed.
        /// </summary>
        /// <typeparam name="T">The Type for which the data is set. If the type is wrong null will be returned.</typeparam>
        /// <returns>The data typed.</returns>
        public T GetData<T>() where T : class
        {
            return Data as T;
        }

        /// <summary>
        /// Gets the Data untyped.
        /// </summary>
        /// <returns>The data untyped.</returns>
        public object GetData()
        {
            return Data;
        }

        /// <summary>
        /// Check if the Data is still valid.
        /// </summary>
        /// <returns>Valid if the validDuration hasn't passed.</returns>
        public bool IsValid()
        {
            return _expiration > DateTime.Now;
        }
    }
}
