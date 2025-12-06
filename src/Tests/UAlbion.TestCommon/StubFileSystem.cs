using System;
using System.Collections.Generic;
using System.IO;
using UAlbion.Api;

namespace UAlbion.TestCommon;

public class StubFileSystem : IFileSystem
{
    public bool IsReadOnly => false;
    public string CurrentDirectory => "";
    public IFileSystem Duplicate(string currentDirectory) => throw new NotSupportedException();
    public bool FileExists(string path) => throw new NotSupportedException();
    public bool DirectoryExists(string path) => throw new NotSupportedException();
    public IEnumerable<string> EnumerateFiles(string path, string filter = null) => throw new NotSupportedException();
    public IEnumerable<string> EnumerateDirectories(string path) => throw new NotSupportedException();
    public void CreateDirectory(string path) => throw new NotSupportedException();
    public Stream OpenRead(string path) => throw new NotSupportedException();
    public Stream OpenWriteTruncate(string path) => throw new NotSupportedException();
    public void DeleteFile(string path) => throw new NotSupportedException();
    public string ReadAllText(string path) => throw new NotSupportedException();
    public void WriteAllText(string path, string fullText) => throw new NotSupportedException();
    public IEnumerable<string> ReadAllLines(string path) => throw new NotSupportedException();
    public byte[] ReadAllBytes(string path) => throw new NotSupportedException();
    public void WriteAllBytes(string path, ReadOnlySpan<byte> bytes) => throw new NotSupportedException();
    public string ToAbsolutePath(string path) => throw new NotImplementedException();
}