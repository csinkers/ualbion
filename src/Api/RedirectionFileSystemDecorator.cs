using System;
using System.Collections.Generic;
using System.IO;

namespace UAlbion.Api;

public class RedirectionFileSystemDecorator : IFileSystem
{
    readonly IFileSystem _disk;
    readonly string _prefix;
    readonly string _target;

    public RedirectionFileSystemDecorator(IFileSystem disk, string prefix, string target)
    {
        if (string.IsNullOrEmpty(prefix)) throw new ArgumentException("Value cannot be null or empty.", nameof(prefix));
        if (string.IsNullOrEmpty(target)) throw new ArgumentException("Value cannot be null or empty.", nameof(target));
        _disk = disk ?? throw new ArgumentNullException(nameof(disk));
        _prefix = prefix;
        _target = target;
    }

    public bool IsReadOnly => _disk.IsReadOnly;
    public string CurrentDirectory => _disk.CurrentDirectory;
    public IFileSystem Duplicate(string currentDirectory) => _disk.Duplicate(currentDirectory);
    public bool FileExists(string path) => _disk.FileExists(ToAbsolutePath(path));
    public bool DirectoryExists(string path) => _disk.DirectoryExists(ToAbsolutePath(path));
    public IEnumerable<string> EnumerateFiles(string path, string filter = null) => _disk.EnumerateFiles(ToAbsolutePath(path), filter);
    public IEnumerable<string> EnumerateDirectories(string path) => _disk.EnumerateDirectories(ToAbsolutePath(path));
    public void CreateDirectory(string path) => _disk.CreateDirectory(ToAbsolutePath(path));
    public Stream OpenRead(string path) => _disk.OpenRead(ToAbsolutePath(path));
    public Stream OpenWriteTruncate(string path) => _disk.OpenWriteTruncate(ToAbsolutePath(path));
    public void DeleteFile(string path) => _disk.DeleteFile(ToAbsolutePath(path));
    public string ReadAllText(string path) => _disk.ReadAllText(ToAbsolutePath(path));
    public void WriteAllText(string path, string fullText) => _disk.WriteAllText(ToAbsolutePath(path), fullText);
    public IEnumerable<string> ReadAllLines(string path) => _disk.ReadAllLines(ToAbsolutePath(path));
    public byte[] ReadAllBytes(string path) => _disk.ReadAllBytes(ToAbsolutePath(path));
    public void WriteAllBytes(string path, byte[] bytes) => _disk.WriteAllBytes(ToAbsolutePath(path), bytes);
    public string ToAbsolutePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return CurrentDirectory;

        if (path.StartsWith(_prefix, StringComparison.InvariantCultureIgnoreCase))
        {
            path = path[(_prefix.Length + 1)..];
            return ApiUtil.CombinePaths(_target, path);
        }

        return ApiUtil.CombinePaths(_disk.CurrentDirectory, path);
    }
}
