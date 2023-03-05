using System;
using System.Collections.Generic;
using System.IO;

namespace UAlbion.Api;

public class ReadOnlyFileSystem : IFileSystem
{
    string _currentDirectory;
    public ReadOnlyFileSystem(string currentDirectory) => CurrentDirectory = currentDirectory;
    public IEnumerable<string> EnumerateDirectory(string path, string filter = null)
        => filter == null ? Directory.EnumerateFiles(path) : Directory.EnumerateFiles(path, filter);

    public bool IsReadOnly => true;
    public string CurrentDirectory
    {
        get => _currentDirectory;
        set
        {
            if (!DirectoryExists(value))
                throw new InvalidOperationException($"Tried to set current directory to \"{value}\", but the directory does not exist");

            _currentDirectory = value;
        }
    }

    public override string ToString() => $"FileSystem @ {_currentDirectory} (ReadOnly)";
    public IFileSystem Duplicate(string currentDirectory) => new ReadOnlyFileSystem(currentDirectory);
    public bool DirectoryExists(string path)             => Directory.Exists(ToAbsolutePath(path));
    public bool FileExists(string path)                  => File.Exists(ToAbsolutePath(path));
    public Stream OpenRead(string path)                  => File.OpenRead(ToAbsolutePath(path));
    public string ReadAllText(string path)               => File.ReadAllText(ToAbsolutePath(path));
    public IEnumerable<string> ReadAllLines(string path) => File.ReadAllLines(ToAbsolutePath(path));
    public byte[] ReadAllBytes(string path)              => File.ReadAllBytes(ToAbsolutePath(path));

    public void CreateDirectory(string path)             => throw new NotSupportedException();
    public void DeleteFile(string path)                  => throw new NotSupportedException();
    public Stream OpenWriteTruncate(string path)         => throw new NotSupportedException();
    public void WriteAllText(string path, string fullText) => throw new NotSupportedException();
    public void WriteAllBytes(string path, byte[] bytes) => throw new NotSupportedException();
    public string ToAbsolutePath(string path) => ApiUtil.CombinePaths(_currentDirectory, path);
}