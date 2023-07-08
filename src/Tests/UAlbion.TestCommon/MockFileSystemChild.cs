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
    public bool FileExists(string path) => _parent.FileExists(ToAbsolutePath(path));
    public bool DirectoryExists(string path) => _parent.DirectoryExists(ToAbsolutePath(path));
    public IEnumerable<string> EnumerateFiles(string path, string filter = null) => _parent.EnumerateFiles(ToAbsolutePath(path), filter);
    public IEnumerable<string> EnumerateDirectories(string path) => _parent.EnumerateDirectories(ToAbsolutePath(path));
    public void CreateDirectory(string path) => _parent.CreateDirectory(ToAbsolutePath(path));
    public Stream OpenRead(string path) => _parent.OpenRead(ToAbsolutePath(path));
    public Stream OpenWriteTruncate(string path) => _parent.OpenWriteTruncate(ToAbsolutePath(path));
    public void DeleteFile(string path) => _parent.DeleteFile(ToAbsolutePath(path));
    public string ReadAllText(string path) => _parent.ReadAllText(ToAbsolutePath(path));
    public void WriteAllText(string path, string fullText) => _parent.WriteAllText(ToAbsolutePath(path), fullText);
    public IEnumerable<string> ReadAllLines(string path) => _parent.ReadAllLines(ToAbsolutePath(path));
    public byte[] ReadAllBytes(string path) => _parent.ReadAllBytes(ToAbsolutePath(path));
    public void WriteAllBytes(string path, byte[] bytes) => _parent.WriteAllBytes(ToAbsolutePath(path), bytes);
    public string ToAbsolutePath(string path) => ApiUtil.CombinePaths(_currentDirectory, path);
}