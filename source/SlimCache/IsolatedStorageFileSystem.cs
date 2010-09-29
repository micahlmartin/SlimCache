using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using Littlefish;

namespace SlimCache
{
    public class IsolatedStorageFileSystem : IFileSystem
    {
        private readonly IsolatedStorageFile _store;
        private const string IsolatedStoreRootDir = "m_RootDir";

        public IsolatedStorageFileSystem(IsolatedStorageFile store)
        {
            _store = store;
        }

        public bool FileExists(string path)
        {
            return _store.FileExists(path);
        }

        public bool DirectoryExists(string path)
        {
            return _store.DirectoryExists(path);
        }

        public Stream CreateFileStream(string path, System.IO.FileMode mode)
        {
            return new IsolatedStorageFileStream(path, mode, _store);
        }

        public Stream TryCreateFileStream(string path, FileMode mode)
        {
            try
            {
                return new IsolatedStorageFileStream(path, mode, _store);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void DeleteFile(string path)
        {
            _store.DeleteFile(path);
        }

        public IEnumerable<string> GetFileNames(string path, string searchTerm)
        {
            return _store.GetFileNames(Path.Combine(path, searchTerm));
        }

        public void CreateDirectory(string path)
        {
            _store.CreateDirectory(path);
        }

        public void DeleteDirectory(string path)
        {
            _store.DeleteDirectory(path);
        }

        public DateTimeOffset GetLastAccessTime(string path)
        {
            return _store.GetLastAccessTime(path);
        }

        public DateTimeOffset GetLastWriteTime(string path)
        {
            return _store.GetLastWriteTime(path);
        }

        public long GetFileSize(string path)
        {
            return GetFileInfo(path, _store).Length;
        }

        public long GetDirectorySize(string path)
        {
            return DirSize(path);
        }

        public long DirSize(string path)
        {
            long size = 0;

            _store.GetFileNames(Path.Combine(path, "*")).ForEach(x => size += GetFileInfo(Path.Combine(path, x), _store).Length);

            _store.GetDirectoryNames(Path.Combine(path, "*")).ForEach(x => size += DirSize(Path.Combine(path, x)));

            return size;
        }

        private static FileInfo GetFileInfo(string path, IsolatedStorageFile store)
        {
            return new FileInfo(GetFullyQualifiedFileName(path, store));
        }
        private static string GetFullyQualifiedFileName(string path, IsolatedStorageFile store)
        {
            return Path.Combine(store.GetType().GetField(IsolatedStorageFileSystem.IsolatedStoreRootDir, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(store).ToString(), path);
        }

        //Workaround for file locking issue in isolated storage file stream.
        /*
            lockStream = new IsolatedStorageFileStream("q.lck", FileMode.OpenOrCreate, isoStore);
    FileStream m_fs = typeof(IsolatedStorageFileStream).InvokeMember(("m_fs"), BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, lockStream, null) as FileStream;
    m_fs.Lock(0, lockStream.Length);
        */

    }   
}
