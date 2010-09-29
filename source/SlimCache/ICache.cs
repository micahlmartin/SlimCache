using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimCache
{
    public interface ICache
    {
        void Add<T>(T entry, string key, DateTime absoluteExpiration) where T: class;
        T Get<T>(string key) where T : class;
        void Remove(string key);
        void Empty();
        IEnumerable<string> Keys { get; }
        bool Exists(string key);
    }
}
