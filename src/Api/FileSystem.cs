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
    public bool DirectoryExists(string path)              => Directory.Exists(ToAbsolutePath(path));
    public void CreateDirectory(string path)              => Directory.CreateDirectory(ToAbsolutePath(path));
    public bool FileExists(string path)                   => File.Exists(ToAbsolutePath(path));
    public void DeleteFile(string path)                   => File.Delete(ToAbsolutePath(path));
    public Stream OpenRead(string path)                   => File.OpenRead(ToAbsolutePath(path));
    public Stream OpenWriteTruncate(string path)          => File.Open(ToAbsolutePath(path), FileMode.Create, FileAccess.ReadWrite);
    public string ReadAllText(string path)                => File.ReadAllText(ToAbsolutePath(path));
    public void WriteAllText(string path, string text)    => File.WriteAllText(ToAbsolutePath(path), text);
    public IEnumerable<string> ReadAllLines(string path)  => File.ReadAllLines(ToAbsolutePath(path));
    public byte[] ReadAllBytes(string path)               => File.ReadAllBytes(ToAbsolutePath(path));
    public void WriteAllBytes(string path, byte[] bytes)  => File.WriteAllBytes(ToAbsolutePath(path), bytes);
    public IEnumerable<string> EnumerateDirectory(string path, string filter = null)
        => filter == null ? Directory.EnumerateFiles(ToAbsolutePath(path)) : Directory.EnumerateFiles(ToAbsolutePath(path), filter);
    public string ToAbsolutePath(string path) => ApiUtil.CombinePaths(_currentDirectory, path);
}