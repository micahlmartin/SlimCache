using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlimCache;
using System.Runtime.Serialization;

namespace SlimCacheTests
{
    [TestClass]
    public class StandardFileSystemCacheTests
    {
        private ICache _cache;

        [TestInitialize]
        public void TestInit()
        {
            _cache = new FileSystemCache(new StandardFileSystem());
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

        [TestMethod]
        public void WhenItemIsExpired_ThenNullIsReturned()
        {
            _cache.Add("test", "testKey", DateTime.Now.AddDays(-1));
            Assert.IsNull(_cache.Get<string>("testkey"));
        }
        
        [TestMethod]
        public void WhenItemHasNoExpiration_ThenTheItemIsReturn()
        {
            _cache.Add("test", "testkey", Cache.NoAbsoluteExpiration);

            Assert.IsNotNull(_cache.Get<string>("testkey"));
        }

        [DataContract]
        private class TestData
        {
            [DataMember]
            public string String1 { get; set; }

            [DataMember]
            public int Int1 { get; set; }

            [DataMember]
            public double Double1 { get; set; }
        }
    }
}
