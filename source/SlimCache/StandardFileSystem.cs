using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Littlefish;

namespace SlimCache
{
    public class StandardFileSystem : IFileSystem
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public Stream CreateFileStream(string path, System.IO.FileMode mode)
        {
            return new FileStream(path, mode);
        }

        public Stream TryCreateFileStream(string path, System.IO.FileMode mode)
        {
            try
            {
                return new FileStream(path, mode);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public IEnumerable<string> GetFileNames(string path, string searchTerm)
        {
            return Directory.GetFiles(path, searchTerm);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public void DeleteDirectory(string path)
        {
            Directory.Delete(path, true);
        }

        public DateTimeOffset GetLastAccessTime(string path)
        {
            return new DateTimeOffset(File.GetLastAccessTime(path));
        }

        public DateTimeOffset GetLastWriteTime(string path)
        {
            return new DateTimeOffset(File.GetLastWriteTime(path));
        }

        public long GetFileSize(string path)
        {
            return new FileInfo(path).Length;
        }

        public long GetDirectorySize(string path)
        {
            return DirSize(new DirectoryInfo(path));
        }

        public static long DirSize(DirectoryInfo directory)
        {
            long size = 0;

            directory.GetFiles().ForEach(x => size += x.Length);

            directory.GetDirectories().ForEach(x => size += DirSize(x));

            return size;
        }
    }
}
