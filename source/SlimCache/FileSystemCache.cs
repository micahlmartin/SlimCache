﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization.Json;
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
        private readonly CacheOptions _options;
        private IFileCacheCleaner _cleaner;
        internal const string CacheDirectory = "_CACHE_";

        public FileSystemCache(IFileSystem fileSystem) : this(fileSystem, CacheOptions.DefaultOptions) { }
        public FileSystemCache(IFileSystem fileSystem, CacheOptions options)
        {
            if (fileSystem == null)
                throw new ArgumentNullException("fileSystem");

            FileSystem = fileSystem;
            _options = options;

            Initialize();
        }

        public void Add<T>(T entry, string key, DateTime absoluteExpiration) where T: class
        {
            //Clean out any existing items in the cache with the same key.
            DeleteExistingCacheItems(key);

            key = GetFilePath(key, absoluteExpiration);

            //TODO: Try to find a better more efficient way of cleaning up disk space other than every time an object is requested.
            CleanUp();
            
            var serializer = new DataContractJsonSerializer(typeof(T));

            //TODO: Add error handling to make sure writing to disk doesn't fail
            try
            {
                using (var stream = FileSystem.CreateFileStream(key, FileMode.CreateNew))
                {
                    serializer.WriteObject(stream, entry);
                }
            }
            catch (Exception)
            {
                //For now, if writing to disk fails, then we'll just ignore the exception
                //The item will still be added to the memory cache, just not persisted to disk.
            }

            MemoryCache.Add(entry, key, absoluteExpiration);
        }
        public T Get<T>(string key) where T: class
        {
            //If the file doesn't exist then the item has expired and been deleted so get out
            var fileName = GetFilePathFromKey(key);
            if(fileName == null || !FileSystem.FileExists(fileName))
                return default(T);

            T obj;

            //If the item has expired, get out
            var cachInfo = new FileCacheItemInfo(fileName);
            if (cachInfo.IsExpired)
                return default(T);

            //If the item exists in the memory cache then return it
            obj = MemoryCache.Get<T>(key);
            if (obj != default(T))
                return obj;

            //The file is not yet in the memory cache so lets deserialize,
            //store it the memory cache then return it
            var cacheItemInfo = new FileCacheItemInfo(Path.GetFileName(fileName));

            //TODO: Add error handling in case we can't open the file for some reason
            try
            {
                using (var fs = FileSystem.CreateFileStream(fileName, FileMode.Open))
                {
                    var serializer = new DataContractJsonSerializer(typeof(T));
                    obj = (T)serializer.ReadObject(fs);
                }
            }
            catch (Exception)
            {
                //For now, if reading from disk fails, then we'll just ignore the exception
                return obj;
            }


            MemoryCache.Add(obj, key, cacheItemInfo.Expiration);

            return obj;
        }
        public void Remove(string key)
        {
            key = GetFilePathFromKey(key);

            try
            {
                if (FileSystem.FileExists(key))
                    FileSystem.DeleteFile(key);
            }
            catch (Exception)
            {
                //for now, if deleting from disk fails, just ignore the error.
            }
        }
        public IEnumerable<string> Keys
        {
            get
            {
                return FileSystem.GetFileNames(CacheDirectory, "*.dat").Select(x => new FileInfo(x).Name.Split('!').ElementAt(0));
            }
        }
        public void Empty()
        {
            foreach (var file in FileSystem.GetFileNames(CacheDirectory, "*.dat"))
            {
                var fqfn = GetFullyQualifiedFileName(file);

                try
                {
                    FileSystem.DeleteFile(fqfn);
                }
                catch (Exception)
                {
                    //for now, if deleting from disk fails, just ignore the error.
                }
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
            var usedSize = FileSystem.GetDirectorySize(CacheDirectory);
            var maxSize = Utils.ConvertStorageUnit(_options.MaxSize, Utils.StorageUnit.Megabyte, Utils.StorageUnit.Bytes);

            var currentloadFactor = usedSize == 0 ? 0 : usedSize / maxSize;
            var loadFactorDelta = currentloadFactor - _options.LoadFactor;

            long spaceToFreeUp = 0;
            if (loadFactorDelta > 0)
                spaceToFreeUp = (long)Math.Round((loadFactorDelta + 0.05) * usedSize, 0);

            _cleaner.CleanUp(spaceToFreeUp);
        }
        private void DeleteExistingCacheItems(string key)
        {
            MemoryCache.Remove(key);
            var path = BuildKey(key);

            if (FileSystem.FileExists(path))
                FileSystem.DeleteFile(path);
        }
        private void Initialize()
        {
            _cleaner = FileCacheCleanerFactory.GetCleaner(_options.ExpirationType, FileSystem);
            MemoryCache = new MemoryCache(MemoryCacheItemExpiredCallback);

            if (!FileSystem.DirectoryExists(CacheDirectory))
                FileSystem.CreateDirectory(CacheDirectory);
        }
        private void MemoryCacheItemExpiredCallback(string key)
        {
            if (Exists(key))
                Remove(key);
        }
        internal string GetFilePathFromKey(string key)
        {
            var fileName = FileSystem.GetFileNames(CacheDirectory, key + "*.dat").FirstOrDefault();
            if (fileName == null)
                return null;

            return GetFullyQualifiedFileName(fileName);
        }

        internal static string GetFilePath(string key, DateTime expiration)
        {
            return Path.Combine(CacheDirectory, string.Join("!", new[] { key, expiration.ToString("ddMMyyyyhhmmsss"), ".dat" }));
        }
        internal static string GetFullyQualifiedFileName(string fileName)
        {
            if (fileName.StartsWith(CacheDirectory))
                return fileName;

            return Path.Combine(CacheDirectory, fileName);
        }

        /* Used for testing */
        internal ICache MemoryCache { get; set; }
        internal IFileSystem FileSystem { get; set; }
    }
}
