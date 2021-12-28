using System.Collections.Generic;
using System.IO;

namespace UAlbion.Api;

public interface IFileSystem
{
    bool FileExists(string path);
    bool DirectoryExists(string path);
    IEnumerable<string> EnumerateDirectory(string path, string filter = null);
    void CreateDirectory(string path);
    Stream OpenRead(string path);
    Stream OpenWriteTruncate(string path);
    void DeleteFile(string path);
    string ReadAllText(string path);
    void WriteAllText(string path, string fullText);
    IEnumerable<string> ReadAllLines(string path);
    byte[] ReadAllBytes(string path);
    void WriteAllBytes(string path, byte[] bytes);
}