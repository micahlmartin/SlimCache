using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimCache
{
    public class CacheOptions
    {
        static CacheOptions()
        {
            DefaultOptions = new CacheOptions { ExpirationType = CacheExpirationType.LeastRecentlyUsed, LoadFactor = 80, MaxSize = 25 };
        }

        public CacheExpirationType ExpirationType { get; set; }
        public double LoadFactor { get; set; }
        public long MaxSize { get; set; }

        internal static CacheOptions DefaultOptions;
    }
}
