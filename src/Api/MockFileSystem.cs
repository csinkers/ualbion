using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace UAlbion.Api;

public class MockFileSystem : IFileSystem
{
    static readonly char[] SeparatorChars = ['\\', '/'];
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

    sealed class DirNode(string path) : Dictionary<string, INode>, INode
    {
        public string Path => path;
        public override string ToString() => path;
    }

    sealed class FileNode(string path) : INode
    {
        public string Path => path;
        public MemoryStream Stream { get; } = new();
        public override string ToString() => $"{path} ({Stream.Length} bytes)";
    }

    public IFileSystem Duplicate(string currentDirectory) => new MockFileSystemChild(this, currentDirectory);
    public string ToAbsolutePath(string path) => ApiUtil.CombinePaths(_currentDirectory, path);

    INode GetDir(string path) // This method assumes that the path is absolute
    {
        lock (_syncRoot)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (path.Length > 260) throw new PathTooLongException();

            INode node = _root;
            foreach (var part in path.Split(SeparatorChars, StringSplitOptions.RemoveEmptyEntries))
            {
                if (node is not DirNode dir) return null;
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
        path = ToAbsolutePath(path);
        lock (_syncRoot)
        {
            if (GetDir(Path.GetDirectoryName(path)) is not DirNode dir)
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
            path = ToAbsolutePath(path);
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
            path = ToAbsolutePath(path);
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
            path = ToAbsolutePath(path);
            return GetDir(path) is DirNode;
        }
    }

    public IEnumerable<string> EnumerateFiles(string path, string filter = null)
    {
        lock (_syncRoot)
        {
            path = ToAbsolutePath(path);
            if (GetDir(path) is not DirNode dir)
                yield break;

            var regex = filter == null ? null : FilterToRegex(filter);

            if (Directory.Exists(path))
            {
                var actualFiles = filter == null
                        ? Directory.EnumerateFiles(path)
                        : Directory.EnumerateFiles(path, filter);

                foreach (var filePath in actualFiles)
                {
                    if (!_maskingFunc(filePath)) // If it's hidden, ignore it
                        continue;

                    var filename = Path.GetFileName(filePath);
                    if (dir.ContainsKey(filename)) // If a mocked copy doesn't exist yet, create one
                        continue;

                    yield return filePath;
                }
            }

            foreach (var kvp in dir)
                if (kvp.Value is FileNode && (regex?.IsMatch(kvp.Key) ?? true))
                    yield return kvp.Value.Path;
        }
    }

    public IEnumerable<string> EnumerateDirectories(string path)
    {
        lock (_syncRoot)
        {
            path = ToAbsolutePath(path);
            if (GetDir(path) is not DirNode dir)
                yield break;

            if (Directory.Exists(path))
            {
                var actualDirectories = Directory.EnumerateDirectories(path);

                foreach (var filePath in actualDirectories)
                {
                    if (!_maskingFunc(filePath)) // If it's hidden, ignore it
                        continue;

                    var filename = Path.GetFileName(filePath);
                    if (dir.ContainsKey(filename)) // If a mocked copy doesn't exist yet, create one
                        continue;

                    yield return filePath;
                }
            }

            foreach (var kvp in dir)
                if (kvp.Value is DirNode dirNode)
                    yield return dirNode.Path;
        }
    }

    public Stream OpenRead(string path)
    {
        lock (_syncRoot)
        {
            path = ToAbsolutePath(path);
            return GetFile(path) switch
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
            path = ToAbsolutePath(path);
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
            path = ToAbsolutePath(path);
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
            path = ToAbsolutePath(path);
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
            path = ToAbsolutePath(path);
            using var s = OpenWriteTruncate(path);
            using var sw = new StreamWriter(s);
            sw.Write(fullText);
        }
    }

    public IEnumerable<string> ReadAllLines(string path)
    {
        lock (_syncRoot)
        {
            path = ToAbsolutePath(path);
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
            path = ToAbsolutePath(path);
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
            path = ToAbsolutePath(path);
            using var s = OpenWriteTruncate(path);
            using var bw = new BinaryWriter(s);
            bw.Write(bytes);
        }
    }

    public string ToStringRecursive()
    {
        lock (_syncRoot)
        {
            StringBuilder sb = new StringBuilder();
            ToStringInner(sb, _root, 0);
            return sb.ToString();
        }
    }

    static void ToStringInner(StringBuilder sb, INode t, int level)
    {
        for (int i = 0; i < level - 1; i++)
            sb.Append("| ");
        if (level > 0)
            sb.Append("+-");

        sb.AppendLine(t.Path);

        if (t is DirNode dir)
            foreach (var child in dir.Values.OrderBy(x => x.Path))
                ToStringInner(sb, child, level + 1);
    }
}
