using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimCache
{
    public interface ICache
    {
        void Add<T>(T entry, string key, DateTime absoluteExpiration);
        T Get<T>(string key);
        void Remove(string key);
        void Empty();
        IEnumerable<string> Keys { get; }
        bool Exists(string key);
    }
}
