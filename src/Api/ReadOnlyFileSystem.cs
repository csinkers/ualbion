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
    public bool DirectoryExists(string path)             => Directory.Exists(ToAbs(path));
    public bool FileExists(string path)                  => File.Exists(ToAbs(path));
    public Stream OpenRead(string path)                  => File.OpenRead(ToAbs(path));
    public string ReadAllText(string path)               => File.ReadAllText(ToAbs(path));
    public IEnumerable<string> ReadAllLines(string path) => File.ReadAllLines(ToAbs(path));
    public byte[] ReadAllBytes(string path)              => File.ReadAllBytes(ToAbs(path));

    public void CreateDirectory(string path)             => throw new NotSupportedException();
    public void DeleteFile(string path)                  => throw new NotSupportedException();
    public Stream OpenWriteTruncate(string path)         => throw new NotSupportedException();
    public void WriteAllText(string path, string text)   => throw new NotSupportedException();
    public void WriteAllBytes(string path, byte[] bytes) => throw new NotSupportedException();

    string ToAbs(string path)
    {
        if (string.IsNullOrEmpty(path))
            return _currentDirectory;

        return Path.IsPathRooted(path) 
            ? path 
            : Path.Combine(_currentDirectory, path);
    }
}