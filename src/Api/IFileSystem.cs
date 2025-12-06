using System;
using System.Collections.Generic;
using System.IO;

namespace UAlbion.Api;

public interface IFileSystem
{
    bool IsReadOnly { get; }
    string CurrentDirectory { get; }
    IFileSystem Duplicate(string currentDirectory);
    bool FileExists(string path);
    bool DirectoryExists(string path);
    IEnumerable<string> EnumerateFiles(string path, string filter = null);
    IEnumerable<string> EnumerateDirectories(string path);
    void CreateDirectory(string path);
    Stream OpenRead(string path);
    Stream OpenWriteTruncate(string path);
    void DeleteFile(string path);
    string ReadAllText(string path);
    void WriteAllText(string path, string fullText);
    IEnumerable<string> ReadAllLines(string path);
    byte[] ReadAllBytes(string path);
    void WriteAllBytes(string path, ReadOnlySpan<byte> bytes);
    string ToAbsolutePath(string path);
}