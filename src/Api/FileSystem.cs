using System;
using System.Collections.Generic;
using System.IO;

namespace UAlbion.Api;

public class FileSystem : IFileSystem
{
    string _currentDirectory;
    public FileSystem(string currentDirectory) => CurrentDirectory = currentDirectory;

    public bool IsReadOnly => false;
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

    public override string ToString() => $"FileSystem @ {_currentDirectory}";
    public IFileSystem Duplicate(string currentDirectory) => new FileSystem(currentDirectory);
    public bool DirectoryExists(string path)              => Directory.Exists(ToAbs(path));
    public void CreateDirectory(string path)              => Directory.CreateDirectory(ToAbs(path));
    public bool FileExists(string path)                   => File.Exists(ToAbs(path));
    public void DeleteFile(string path)                   => File.Delete(ToAbs(path));
    public Stream OpenRead(string path)                   => File.OpenRead(ToAbs(path));
    public Stream OpenWriteTruncate(string path)          => File.Open(ToAbs(path), FileMode.Create, FileAccess.ReadWrite);
    public string ReadAllText(string path)                => File.ReadAllText(ToAbs(path));
    public void WriteAllText(string path, string text)    => File.WriteAllText(ToAbs(path), text);
    public IEnumerable<string> ReadAllLines(string path)  => File.ReadAllLines(ToAbs(path));
    public byte[] ReadAllBytes(string path)               => File.ReadAllBytes(ToAbs(path));
    public void WriteAllBytes(string path, byte[] bytes)  => File.WriteAllBytes(ToAbs(path), bytes);
    public IEnumerable<string> EnumerateDirectory(string path, string filter = null)
        => filter == null ? Directory.EnumerateFiles(ToAbs(path)) : Directory.EnumerateFiles(ToAbs(path), filter);

    string ToAbs(string path)
    {
        if (string.IsNullOrEmpty(path))
            return _currentDirectory;

        return Path.IsPathRooted(path) 
            ? path 
            : Path.Combine(_currentDirectory, path);
    }
}