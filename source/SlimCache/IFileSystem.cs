using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SlimCache
{
    public interface IFileSystem
    {
        bool FileExists(string path);
        FileStream CreateFileStream(string path, FileMode mode);
        FileStream TryCreateFileStream(string path, FileMode mode);
        void DeleteFile(string path);
        IEnumerable<string> GetFileNames(string path, string searchTerm);
        DateTimeOffset GetLastAccessTime(string path);
        DateTimeOffset GetLastWriteTime(string path);
        long GetFileSize(string path);

        bool DirectoryExists(string path);
        void CreateDirectory(string path);
        void DeleteDirectory(string path);
        long GetDirectorySize(string path);
    }
}
