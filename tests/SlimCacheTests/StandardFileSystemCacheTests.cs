using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlimCache;
using System.Runtime.Serialization;
using Moq;
using System.IO;
using System.Runtime.Serialization.Json;
using SlimCacheTests.Data;

namespace SlimCacheTests
{
    [TestClass]
    public class StandardFileSystemCacheTests
    {
        private FileSystemCache _cache;

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

        [TestMethod]
        public void WhenAnItemIsAddedToTheCache_ThenItIsAlsoAddedToTheMemoryCache()
        {
            Mock<ICache> iMemCacheMock = new Mock<ICache>();
            _cache.MemoryCache = iMemCacheMock.Object;

            _cache.Add("item", "test", DateTime.Now);

            iMemCacheMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once());
        }

        [TestMethod]
        public void WhenAnItemIsRetrievedFromTheCacheThatExistsInTheMemoryCache_ThenTheItemIsNeverReadFromDisk()
        {
            Mock<ICache> imemCacheMock = new Mock<ICache>();
            Mock<IFileSystem> iFileSystemMock = new Mock<IFileSystem>();
            _cache.MemoryCache = imemCacheMock.Object;
            _cache.FileSystem = iFileSystemMock.Object;

            imemCacheMock.Setup(x => x.Get<string>(It.IsAny<string>())).Returns("Test");
            iFileSystemMock.Setup(x => x.GetFileNames(It.IsAny<string>(), It.IsAny<string>())).Returns(new[] { "test!"  + DateTime.Now.AddDays(1).ToString("ddMMyyyyhhmmss") +"!.dat", "test!01012010000000.dat" });
            iFileSystemMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            _cache.Get<string>("test");

            imemCacheMock.Verify(x => x.Get<string>(It.IsAny<string>()), Times.Once());
            iFileSystemMock.Verify(x => x.CreateFileStream(It.IsAny<string>(), It.IsAny<FileMode>()), Times.Never());
        }

        [TestMethod]
        public void WhenAnItemIsRetrievedFromTheCacheThatDoesNotExist_ThenItIsStoredInTheMemoryCache()
        {
            Mock<ICache> imemCacheMock = new Mock<ICache>();
            Mock<IFileSystem> iFileSystemMock = new Mock<IFileSystem>();
            _cache.MemoryCache = imemCacheMock.Object;
            _cache.FileSystem = iFileSystemMock.Object;

            iFileSystemMock.Setup(x => x.GetFileNames(It.IsAny<string>(), It.IsAny<string>())).Returns(new[] { "test!" + DateTime.Now.AddDays(1).ToString("ddMMyyyyhhmmss") + "!.dat", "test!01012010000000.dat" });
            iFileSystemMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            var stream = new MemoryStream();
            var serializer = new DataContractJsonSerializer(typeof(string));
            serializer.WriteObject(stream, "test");
            stream.Flush();
            stream.Position = 0;

            iFileSystemMock.Setup(x => x.CreateFileStream(It.IsAny<string>(), It.IsAny<FileMode>())).Returns(stream);
            _cache.Get<string>("test");

            imemCacheMock.Verify(x => x.Get<string>(It.IsAny<string>()), Times.Once());
            iFileSystemMock.Verify(x => x.CreateFileStream(It.IsAny<string>(), It.IsAny<FileMode>()), Times.Once());
            imemCacheMock.Verify(x => x.Add<string>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()));

            stream.Dispose();
        }
    }
}
