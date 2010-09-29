using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlimCache;

namespace SlimCacheTests
{
    [TestClass]
    public class StandardFileSystemTests
    {
        private IFileSystem _fs;

        [TestInitialize]
        public void InitTest()
        {
            _fs = new StandardFileSystem();
        }

        [TestMethod]
        public void WhenDirectoryIsCreated_ThenDirectoryExistsIsTrue()
        {
            _fs.CreateDirectory("TestDirectory");
            Assert.IsTrue(_fs.DirectoryExists("TestDirectory"));
        }

        [TestMethod]
        public void WhenSubDirectoriesAreCreated_ThenDirectoryExistsIsTrue()
        {
            _fs.CreateDirectory("TopLevel");
            Assert.IsTrue(_fs.DirectoryExists("TopLevel"));

            _fs.CreateDirectory("TopLevel/SubDirectory1");
            Assert.IsTrue(_fs.DirectoryExists("TopLevel/SubDirectory1"));

            _fs.CreateDirectory("TopLevel/SubDirectory2");
            Assert.IsTrue(_fs.DirectoryExists("TopLevel/SubDirectory2"));

            _fs.CreateDirectory("TopLevel/SubDirectory2/LeafDirectory");
            Assert.IsTrue(_fs.DirectoryExists("TopLevel/SubDirectory2/LeafDirectory"));
        }

        [TestMethod]
        public void DirectorySizeIsCalculatedProperly()
        {
            _fs.CreateDirectory("Root");
            _fs.CreateDirectory("Root/Sub1");
            _fs.CreateDirectory("Root/Sub2");
            _fs.CreateDirectory("Root/Sub2/Sub1");

            using (var stream = _fs.CreateFileStream("Root/Sub2/Sub1/Test.dat", System.IO.FileMode.OpenOrCreate))
            {
                for (int i = 0; i < 500; i++)
                {
                    stream.WriteByte((byte)i);
                }
            }

            using (var stream = _fs.CreateFileStream("Root/Test.dat", System.IO.FileMode.OpenOrCreate))
            {
                for (int i = 0; i < 500; i++)
                {
                    stream.WriteByte((byte)i);
                }
            }

            var size = _fs.GetDirectorySize("Root");
            Assert.AreEqual(1000, size);
        }
    }
}
