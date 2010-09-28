using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Web;
using SlimCache;
using Littlefish;

namespace SlimCache
{
    //TODO: Need to implement error handling in case of file locking.
    //TODO: Need to make the cache thread-safe. Issues will occure if to threads tried to read/write the same item in the cache at the same time.

    /// <summary>
    /// This class represents a cache that uses the file system as a persistent store. 
    /// Only one instance per application should be created. This class is not thread-safe.
    /// </summary>
    public class FileSystemCache : ICache
    {
        private readonly IFileSystem _fs;
        private readonly CacheOptions _options;
        private IFileCacheCleaner _cleaner;
        internal const string CacheDirectory = "_CACHE_";
        
        public FileSystemCache(IFileSystem fileSystem) : this(fileSystem, CacheOptions.DefaultOptions) { }
        public FileSystemCache(IFileSystem fileSystem, CacheOptions options)
        {
            if (fileSystem == null)
                throw new ArgumentNullException("fileSystem");

            _fs = fileSystem;
            _options = options;

            Initialize();
        }

        public void Add<T>(T entry, string key, DateTime absoluteExpiration)
        {
            DeleteExistingCacheItems(BuildKey(key));

            key = GetFilePath(key, absoluteExpiration);

            //TODO: Need checking before adding things to the cache that we actually have room to do it.
            CleanUp();
            
            var serializer = new DataContractJsonSerializer(typeof(T));

            using (var stream = _fs.CreateFileStream(key, FileMode.CreateNew))
            {
                serializer.WriteObject(stream, entry);
            }
        }
        public T Get<T>(string key)
        {
            //TODO: Need a memory caching solution so that everytime an object is requested it doesn't need to be deserialzed from the file system.

            var fileName = GetFilePathFromKey(key);
            if(fileName == null || !_fs.FileExists(fileName))
                return default(T);

            T obj;

            var cachInfo = new FileCacheItemInfo(fileName);
            if (cachInfo.IsExpired)
                return default(T);

            using(var fs =  _fs.CreateFileStream(fileName, FileMode.Open))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                obj = (T)serializer.ReadObject(fs);           
            }

            return obj;
        }
        public void Remove(string key)
        {
            key = GetFilePathFromKey(key);

            if (_fs.FileExists(key))
                _fs.DeleteFile(key);
        }
        public IEnumerable<string> Keys
        {
            get
            {
                return _fs.GetFileNames(CacheDirectory, "*.dat").Select(x => new FileInfo(x).Name.Split('!').ElementAt(0));
            }
        }
        public void Empty()
        {
            foreach (var file in _fs.GetFileNames(CacheDirectory, "*.dat"))
            {
                var fqfn = GetFullyQualifiedFileName(file);
                _fs.DeleteFile(fqfn);
            }
        }
        public bool Exists(string key)
        {
            return Keys.Contains(key, StringComparer.OrdinalIgnoreCase);
        }

        private string BuildKey(string key)
        {
            return Path.Combine(CacheDirectory, key + "!*.dat");
        }
        private void CleanUp()
        {
            var usedSize = _fs.GetDirectorySize(CacheDirectory);
            var maxSize = Utils.ConvertStorageUnit(_options.MaxSize, Utils.StorageUnit.Megabyte, Utils.StorageUnit.Bytes);

            var currentloadFactor = usedSize == 0 ? 0 : usedSize / maxSize;
            var loadFactorDelta = currentloadFactor - _options.LoadFactor;

            long spaceToFreeUp = 0;
            if (loadFactorDelta > 0)
                spaceToFreeUp = (long)Math.Round((loadFactorDelta + 0.05) * usedSize, 0);

            _cleaner.CleanUp(spaceToFreeUp);
        }
        private void DeleteExistingCacheItems(string path)
        {
            if (_fs.FileExists(path))
                _fs.DeleteFile(path);
        }
        private void Initialize()
        {
            _cleaner = FileCacheCleanerFactory.GetCleaner(_options.ExpirationType, _fs);

            if (!_fs.DirectoryExists(CacheDirectory))
                _fs.CreateDirectory(CacheDirectory);
        }

        internal string GetFilePathFromKey(string key)
        {
            return GetFullyQualifiedFileName(_fs.GetFileNames(CacheDirectory, key + "*.dat").FirstOrDefault());
        }

        internal static string GetFilePath(string key, DateTime expiration)
        {
            return Path.Combine(CacheDirectory, string.Join("!", key, expiration.ToString("ddMMyyyyhhmmsss"), ".dat"));
        }
        internal static string GetFullyQualifiedFileName(string fileName)
        {
            if (fileName.StartsWith(CacheDirectory))
                return fileName;

            return Path.Combine(CacheDirectory, fileName);
        }
    }
}
