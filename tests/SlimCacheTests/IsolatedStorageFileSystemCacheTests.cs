using System;
using System.IO.IsolatedStorage;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using SlimCache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;
using SlimCacheTests.Data;
using System.IO;

namespace SlimCacheTests
{
    [TestClass]
    public class IsolatedStorageFileSystemCacheTests
    {
        private ICache _cache;

        [TestInitialize]
        public void TestInit()
        {
            _cache = new FileSystemCache(new IsolatedStorageFileSystem(IsolatedStorageFile.GetUserStoreForAssembly()));
            _cache.Empty();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _cache.Empty();
        }

        [TestMethod]
        public void WhenAnItemIsAddedToTheCache_ThenKeysContainsTheKeyName()
        {
            _cache.Add("Test", "TestKey", Cache.NoAbsoluteExpiration);

            Assert.IsTrue(_cache.Keys.Contains("testkey", StringComparer.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void WhenAnItemIsAddedToTheCache_ThenExistsIsTrue()
        {
            _cache.Add("test", "TestKey", Cache.NoAbsoluteExpiration);

            Assert.IsTrue(_cache.Exists("testkey"));
        }

        [TestMethod]
        public void WhenAnItemIsClearedFromTheCache_ThenExistsIsFalse()
        {
            _cache.Add("test", "TestKey", Cache.NoAbsoluteExpiration);
            Assert.IsTrue(_cache.Exists("TestKey"));

            _cache.Remove("TestKey");
            Assert.IsFalse(_cache.Exists("TestKey"));
        }

        [TestMethod]
        public void WhenAnItemIsAddedToTheCache_ThenTheSameItemCanBeRetrieved()
        {
            var data = new TestData { String1 = "testString", Double1 = 5.7, Int1 = 45 };

            _cache.Add(data, "testKey", Cache.NoAbsoluteExpiration);

            var returnedData = _cache.Get<TestData>("testKey");
            Assert.IsNotNull(returnedData);
            Assert.AreEqual(data.String1, returnedData.String1);
            Assert.AreEqual(data.Double1, returnedData.Double1);
            Assert.AreEqual(data.Int1, returnedData.Int1);
        }
    }
}
