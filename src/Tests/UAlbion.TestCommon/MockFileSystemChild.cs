using System;
using System.Collections.Generic;
using System.IO;
using UAlbion.Api;

namespace UAlbion.TestCommon;

public class MockFileSystemChild : IFileSystem
{
    readonly MockFileSystem _parent;
    readonly string _currentDirectory;

    public MockFileSystemChild(MockFileSystem parent, string currentDirectory)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        _currentDirectory = currentDirectory;
    }

    public bool IsReadOnly => _parent.IsReadOnly;
    public string CurrentDirectory => _currentDirectory;
    public IFileSystem Duplicate(string currentDirectory) => _parent.Duplicate(currentDirectory);
    public bool FileExists(string path) => _parent.FileExists(ToAbs(path));
    public bool DirectoryExists(string path) => _parent.DirectoryExists(ToAbs(path));
    public IEnumerable<string> EnumerateDirectory(string path, string filter = null) => _parent.EnumerateDirectory(ToAbs(path), filter);
    public void CreateDirectory(string path) => _parent.CreateDirectory(ToAbs(path));
    public Stream OpenRead(string path) => _parent.OpenRead(ToAbs(path));
    public Stream OpenWriteTruncate(string path) => _parent.OpenWriteTruncate(ToAbs(path));
    public void DeleteFile(string path) => _parent.DeleteFile(ToAbs(path));
    public string ReadAllText(string path) => _parent.ReadAllText(ToAbs(path));
    public void WriteAllText(string path, string fullText) => _parent.WriteAllText(ToAbs(path), fullText);
    public IEnumerable<string> ReadAllLines(string path) => _parent.ReadAllLines(ToAbs(path));
    public byte[] ReadAllBytes(string path) => _parent.ReadAllBytes(ToAbs(path));
    public void WriteAllBytes(string path, byte[] bytes) => _parent.WriteAllBytes(ToAbs(path), bytes);

    string ToAbs(string path)
    {
        if (string.IsNullOrEmpty(path))
            return _currentDirectory;

        return Path.IsPathRooted(path) 
            ? path 
            : Path.Combine(_currentDirectory, path);
    }
}