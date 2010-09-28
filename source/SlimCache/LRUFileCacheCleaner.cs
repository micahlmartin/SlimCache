using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimCache;

namespace SlimCache
{
    class LRUFileCacheCleaner : IFileCacheCleaner
    {
        private IFileSystem _fs;
        private IList<FileCacheItemInfo> _cacheItems;

        public LRUFileCacheCleaner(IFileSystem fileSystem)
        {
            _fs = fileSystem;
        }

        public void CleanUp(long spaceToFreeUp)
        {
            _cacheItems = new List<FileCacheItemInfo>();

            LoadCacheItems();

            RemoveExpiredItems();

            RemoveLRUItems(spaceToFreeUp);
        }

        private void RemoveLRUItems(long spaceToFreeUp)
        {
            var tempItems = _cacheItems.OrderBy(x => x.LastUsed);

            long spaceFreed = 0;

            foreach (var item in tempItems)
            {
                if (spaceFreed >= spaceToFreeUp) break;

                _fs.DeleteFile(item.FullyQualifiedFileName);
                spaceFreed += item.Size;
                _cacheItems.Remove(item);
            }
        }

        private void LoadCacheItems()
        {
            var fileNames = _fs.GetFileNames(FileSystemCache.CacheDirectory, "*.dat");
            foreach (var file in fileNames)
            {
                var fqfn = FileSystemCache.GetFullyQualifiedFileName(file);
                var info = new FileCacheItemInfo(file) { LastUsed = _fs.GetLastAccessTime(fqfn), FullyQualifiedFileName = fqfn, Size = _fs.GetFileSize(fqfn)  };
                _cacheItems.Add(info);
            }
        }

        private void RemoveExpiredItems()
        {
            var tempItems = new List<FileCacheItemInfo>(_cacheItems);
            foreach (var item in tempItems)
            {
                if (item.IsExpired)
                {
                    if (_fs.FileExists(item.FullyQualifiedFileName))
                        _fs.DeleteFile(item.FullyQualifiedFileName);

                    _cacheItems.Remove(item);
                }
            }
        }
    }
}
