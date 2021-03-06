using System.Collections.Generic;
using System.IO;

namespace UAlbion.Api
{
    public class FileSystem : IFileSystem
    {
        public IEnumerable<string> EnumerateDirectory(string path, string filter = null)
            => filter == null ? Directory.EnumerateFiles(path) : Directory.EnumerateFiles(path, filter);

        public bool DirectoryExists(string path)             => Directory.Exists(path);
        public void CreateDirectory(string path)             => Directory.CreateDirectory(path);
        public bool FileExists(string path)                  => File.Exists(path);
        public void DeleteFile(string path)                  => File.Delete(path);
        public Stream OpenRead(string path)                  => File.OpenRead(path);
        public Stream OpenWriteTruncate(string path)         => File.Open(path, FileMode.Create, FileAccess.ReadWrite);
        public string ReadAllText(string path)               => File.ReadAllText(path);
        public void WriteAllText(string path, string text)   => File.WriteAllText(path, text);
        public IEnumerable<string> ReadAllLines(string path) => File.ReadAllLines(path);
        public byte[] ReadAllBytes(string path)              => File.ReadAllBytes(path);
        public void WriteAllBytes(string path, byte[] bytes) => File.WriteAllBytes(path, bytes);
    }
}
