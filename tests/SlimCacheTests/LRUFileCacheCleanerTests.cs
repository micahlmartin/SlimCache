using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlimCache;
using System.IO.IsolatedStorage;
using SlimCacheTests.Data;
using System.Threading;

namespace SlimCacheTests
{
    [TestClass]
    public class LRUFileCacheCleanerTests
    {
        private FileSystemCache _cache;
        private IFileCacheCleaner _cleaner;
        private IFileSystem _fs;

        [TestInitialize]
        public void TestInit()
        {
            _fs = new IsolatedStorageFileSystem(IsolatedStorageFile.GetUserStoreForAssembly());

            _cache = new FileSystemCache(_fs);
            _cache.Empty();

            _cleaner = new LRUFileCacheCleaner(_fs);
        }

        private void LoadSimpleData()
        {
            var data = new TestData { Double1 = 4.5, Int1 = 32, String1 = "test" };

            _cache.Add(data, "key1", Cache.NoAbsoluteExpiration);
            _cache.Add(data, "key2", DateTime.Now);
            _cache.Add(data, "key3", DateTime.Now.AddDays(2));
            _cache.Add(data, "key4", DateTime.Now.AddDays(-2));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _cache.Empty();
        }

        [TestMethod]
        public void WhenCacheIsCleanedUp_ThenFilesAreRemoved()
        {
            LoadSimpleData();

            Thread.SpinWait(1);

            _cleaner.CleanUp(0);

            Assert.IsTrue(_fs.GetFileNames(FileSystemCache.CacheDirectory, "key1!*.dat").Count() == 1);
            Assert.IsTrue(_fs.GetFileNames(FileSystemCache.CacheDirectory, "key2!*.dat").Count() == 0);
            Assert.IsTrue(_fs.GetFileNames(FileSystemCache.CacheDirectory, "key3!*.dat").Count() == 1);
            Assert.IsTrue(_fs.GetFileNames(FileSystemCache.CacheDirectory, "key4!*.dat").Count() == 0);
        }

        [TestMethod]
        public void WhenCacheIsCleanUp_ThenTheCorrectAmountOfSpaceIsFreed()
        {
            Assert.AreEqual(0, _fs.GetDirectorySize(FileSystemCache.CacheDirectory));

            LoadSimpleData();

            Thread.SpinWait(1);

            _cleaner.CleanUp(50);

            Assert.IsTrue(_fs.GetFileNames(FileSystemCache.CacheDirectory, "key1!*.dat").Count() == 0);
            Assert.IsTrue(_fs.GetFileNames(FileSystemCache.CacheDirectory, "key2!*.dat").Count() == 0);
            Assert.IsTrue(_fs.GetFileNames(FileSystemCache.CacheDirectory, "key3!*.dat").Count() == 0);
            Assert.IsTrue(_fs.GetFileNames(FileSystemCache.CacheDirectory, "key4!*.dat").Count() == 0);

            Assert.AreEqual(0, _fs.GetDirectorySize(FileSystemCache.CacheDirectory));
        }
    }
}
