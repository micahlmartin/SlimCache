using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace SlimCache
{
    internal class HttpRuntimeMemoryCache : ICache
    {
        private Action<string> _itemExpirationCallback;

        public HttpRuntimeMemoryCache(Action<string> itemExpirationCallback)
        {
            _itemExpirationCallback = itemExpirationCallback;
        }

        public void Add<T>(T entry, string key, DateTime absoluteExpiration) where T : class
        {
            HttpRuntime.Cache.Add(key, entry, null, absoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, ItemRemovedCallback);
        }

        public T Get<T>(string key) where T : class
        {
            try
            {
                return (T)HttpRuntime.Cache["key"];

            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public void Remove(string key)
        {
            HttpRuntime.Cache.Remove(key);
        }

        public void Empty()
        {
            throw new NotSupportedException();
        }

        public IEnumerable<string> Keys
        {
            get { throw new NotSupportedException(); }
        }

        public bool Exists(string key)
        {
            throw new NotSupportedException();
        }

        public void ItemRemovedCallback(string key, object value, CacheItemRemovedReason reason)
        {
            _itemExpirationCallback(key);
        }
    }
}
