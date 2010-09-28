using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimCache;

namespace SlimCache
{
    static class FileCacheCleanerFactory
    {
        public static IFileCacheCleaner GetCleaner(CacheExpirationType expirationType, IFileSystem fileSystem)
        {
            switch (expirationType)
            {
                case CacheExpirationType.LeastRecentlyUsed:
                default:
                    return new LRUFileCacheCleaner(fileSystem);
            }
        }
    }
}
