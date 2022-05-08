using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UAlbion.Api;

namespace UAlbion.TestCommon;

public class MockFileSystem : IFileSystem
{
    static readonly char[] SeparatorChars = { '\\', '/' };
    readonly object _syncRoot = new();
    readonly DirNode _root = new(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "" : "/");
    readonly Func<string, bool> _maskingFunc;
    string _currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="maskingFunc">A function that takes a path, and returns true if
    /// the on-disk version should be used as a fallback for reading, and false if
    /// the on-disk version (if any) should be ignored.</param>
    public MockFileSystem(Func<string, bool> maskingFunc) => _maskingFunc = maskingFunc ?? throw new ArgumentNullException(nameof(maskingFunc));
    public MockFileSystem(bool fallBackToFileSystem) => _maskingFunc = fallBackToFileSystem ? _ => true : _ => false;
    public bool IsReadOnly { get; set; }
    public override string ToString() => $"MockFileSystem @ {_currentDirectory}{(IsReadOnly ? " (ReadOnly)" : "")}";

    interface INode { string Path { get; } }
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

    class DirNode : Dictionary<string, INode>, INode
    {
        public DirNode(string path) => Path = path;
        public string Path { get; }
        public override string ToString() => Path;
    }

    class FileNode : INode
    {
        public FileNode(string path) { Path = path; Stream = new MemoryStream(); }
        public string Path { get; }
        public MemoryStream Stream { get; }
        public override string ToString() => $"{Path} ({Stream.Length} bytes)";
    }

    public IFileSystem Duplicate(string currentDirectory)
    {
        return new MockFileSystemChild(this, currentDirectory);
    }

    INode GetDir(string path)
    {
        lock (_syncRoot)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (path.Length > 260) throw new PathTooLongException();

            INode node = _root;
            foreach (var part in path.Split(SeparatorChars, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!(node is DirNode dir)) return null;
                if (dir.TryGetValue(part, out node)) continue;

                var nodePath = Path.Combine(dir.Path, part);
                if (_maskingFunc(nodePath) && Directory.Exists(nodePath))
                {
                    node = new DirNode(nodePath);
                    dir[part] = node;
                }
                else return null;
            }
            return node;
        }
    }

    (DirNode, INode) GetFile(string path)
    {
        lock (_syncRoot)
        {
            if (!(GetDir(Path.GetDirectoryName(path)) is DirNode dir))
                throw new DirectoryNotFoundException($"Could not find a part of the path '{path}'.");

            var filename = Path.GetFileName(path);
            dir.TryGetValue(filename, out var node);

            if (node == null && _maskingFunc(path) && File.Exists(path))
            {
                var newFile = new FileNode(Path.Combine(dir.Path, filename));
                using var s = File.OpenRead(path);
                s.CopyTo(newFile.Stream);
                dir[filename] = newFile;
                node = newFile;
            }

            return (dir, node);
        }
    }

    public void CreateDirectory(string path)
    {
        if (IsReadOnly)
            throw new NotSupportedException();

        lock (_syncRoot)
        {
            path = Path.GetFullPath(path);
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (path.Length > 260) throw new PathTooLongException();

            INode node = _root;
            foreach (var part in path.Split(SeparatorChars, StringSplitOptions.RemoveEmptyEntries))
            {
                switch (node)
                {
                    case FileNode file:
                        throw new IOException($"Cannot create \"{file.Path}\" because a file or directory with the same name already exists.");
                    case DirNode dir:
                        if (!dir.TryGetValue(part, out node))
                        {
                            node = new DirNode(Path.Combine(dir.Path, part));
                            dir[part] = node;
                        }
                        break;
                }
            }
        }
    }

    static Regex FilterToRegex(string filter)
    {
        var sb = new StringBuilder();
        sb.Append('^');
        foreach (var c in filter)
        {
            switch (c)
            {
                case '*': sb.Append(".*"); break;
                case '?': sb.Append('.'); break;
                case '.': sb.Append("\\."); break;
                default: sb.Append(c); break;
            }
        }
        sb.Append('$');

        return new Regex(sb.ToString());
    }

    public bool FileExists(string path)
    {
        lock (_syncRoot)
        {
            path = Path.GetFullPath(path);
            if (GetDir(Path.GetDirectoryName(path)) is not DirNode dir)
                return false;

            var filename = Path.GetFileName(path);
            var result = dir.TryGetValue(filename, out var node) && node is not DirNode;
            return !result && _maskingFunc(path) ? File.Exists(path) : result;
        }
    }

    public bool DirectoryExists(string path)
    {
        lock (_syncRoot)
        {
            path = Path.GetFullPath(path);
            return GetDir(path) is DirNode;
        }
    }

    public IEnumerable<string> EnumerateDirectory(string path, string filter = null)
    {
        lock (_syncRoot)
        {
            path = Path.GetFullPath(path);
            if (!(GetDir(path) is DirNode dir))
                return Enumerable.Empty<string>();

            var regex = filter == null ? null : FilterToRegex(filter);
            return dir
                .Where(kvp => kvp.Value is FileNode && (regex?.IsMatch(kvp.Key) ?? true))
                .Select(x => x.Value.Path);
        }
    }

    public Stream OpenRead(string path)
    {
        lock (_syncRoot)
        {
            return GetFile(Path.GetFullPath(path)) switch
            {
                ({ }, FileNode file) => new MockFileStream(file.Stream, true),
                ({ }, DirNode) =>
                    throw new UnauthorizedAccessException($"Access to the path '{path}' is denied."),
                _ => throw new FileNotFoundException($"Could not find file '{path}'.")
            };
        }
    }

    public Stream OpenWriteTruncate(string path)
    {
        if (IsReadOnly)
            throw new NotSupportedException();

        lock (_syncRoot)
        {
            path = Path.GetFullPath(path);
            var filename = Path.GetFileName(path);
            switch (GetFile(path))
            {
                case ({ }, FileNode file):
                    file.Stream.Position = 0;
                    file.Stream.SetLength(0);
                    return new MockFileStream(file.Stream);
                case ({ }, DirNode): throw new UnauthorizedAccessException($"Access to the path '{path}' is denied.");
                case ({ } dir, null):
                    var newFile = new FileNode($"{dir.Path}\\{filename}");
                    dir[filename] = newFile;
                    return new MockFileStream(newFile.Stream);
                default: throw new FileNotFoundException($"Could not find file '{path}'.");
            }
        }
    }

    public void DeleteFile(string path)
    {
        if (IsReadOnly)
            throw new NotSupportedException();

        lock (_syncRoot)
        {
            path = Path.GetFullPath(path);
            var (dir, file) = GetFile(path);
            if (file == null)
                return;

            dir.Remove(Path.GetFileName(path));
        }
    }

    public string ReadAllText(string path)
    {
        lock (_syncRoot)
        {
            path = Path.GetFullPath(path);
            using var s = OpenRead(path);
            using var sr = new StreamReader(s);
            return sr.ReadToEnd();
        }
    }

    public void WriteAllText(string path, string fullText)
    {
        if (IsReadOnly)
            throw new NotSupportedException();

        lock (_syncRoot)
        {
            path = Path.GetFullPath(path);
            using var s = OpenWriteTruncate(path);
            using var sw = new StreamWriter(s);
            sw.Write(fullText);
        }
    }

    public IEnumerable<string> ReadAllLines(string path)
    {
        lock (_syncRoot)
        {
            path = Path.GetFullPath(path);
            using var s = OpenRead(path);
            using var sr = new StreamReader(s);
            while (!sr.EndOfStream)
                yield return sr.ReadLine();
        }
    }

    public byte[] ReadAllBytes(string path)
    {
        lock (_syncRoot)
        {
            path = Path.GetFullPath(path);
            using var s = OpenRead(path);
            using var br = new BinaryReader(s);
            return br.ReadBytes((int)s.Length);
        }
    }

    public void WriteAllBytes(string path, byte[] bytes)
    {
        if (IsReadOnly)
            throw new NotSupportedException();

        lock (_syncRoot)
        {
            path = Path.GetFullPath(path);
            using var s = OpenWriteTruncate(path);
            using var bw = new BinaryWriter(s);
            bw.Write(bytes);
        }
    }
}
