using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UAlbion.Api;

namespace UAlbion.Formats.Containers
{
    public class MockFileSystem : IFileSystem
    {
        static readonly char[] SeparatorChars = { '\\', '/' };
        readonly DirNode _root = new DirNode("");
        readonly bool _fallbackToFileSystem;

        public MockFileSystem(bool fallbackToFileSystem) => _fallbackToFileSystem = fallbackToFileSystem;

        interface INode { string Path { get; } }

        class DirNode : Dictionary<string, INode>, INode
        {
            public DirNode(string path) => Path = path;
            public string Path { get; }
        }

        class FileNode : INode
        {
            public FileNode(string path)
            {
                Path = path;
                Stream = new MemoryStream();
            }

            public FileNode(string path, byte[] content)
            {
                if (content == null) throw new ArgumentNullException(nameof(content));
                Path = path;
                Stream = new MemoryStream(content);
            }

            public string Path { get; }
            public MemoryStream Stream { get; }
        }

        INode GetDir(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (path.Length > 260) throw new PathTooLongException();

            INode node = _root;
            foreach (var part in path.Split(SeparatorChars))
            {
                if (!(node is DirNode dir)) return null;
                if (dir.TryGetValue(part, out node)) continue;

                var nodePath = Path.Combine(dir.Path, part);
                if (_fallbackToFileSystem && Directory.Exists(nodePath))
                {
                    node = new DirNode(nodePath);
                    dir[part] = node;
                }
                else return null;
            }
            return node;
        }

        (DirNode, INode) GetFile(string path)
        {
            if (!(GetDir(Path.GetDirectoryName(path)) is DirNode dir))
                throw new DirectoryNotFoundException($"Could not find a part of the path '{path}'.");

            var filename = Path.GetFileName(path);
            dir.TryGetValue(filename, out var node);

            if (node == null && _fallbackToFileSystem && File.Exists(path))
            {
                var newFile = new FileNode($"{dir.Path}\\{filename}");
                using var s = File.OpenRead(path);
                s.CopyTo(newFile.Stream);
                dir[filename] = newFile;
                node = newFile;
            }

            return (dir, node);
        }

        public void CreateDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (path.Length > 260) throw new PathTooLongException();

            INode node = _root;
            foreach (var part in path.Split(SeparatorChars))
            {
                switch (node)
                {
                    case FileNode file:
                        throw new IOException($"Cannot create \"{file.Path}\" because a file or directory with the same name already exists.");
                    case DirNode dir:
                    {
                        if (!dir.TryGetValue(part, out node))
                            dir[part] = new DirNode(FormattableString.Invariant($"dir.Path\\part"));
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
            if (!(GetDir(Path.GetDirectoryName(path)) is DirNode dir))
                return false;

            var filename = Path.GetFileName(path);
            var result = dir.ContainsKey(filename);
            return !result && _fallbackToFileSystem ? File.Exists(path) : result;
        }

        public bool DirectoryExists(string path) => GetDir(path) is DirNode;
        public IEnumerable<string> EnumerateDirectory(string path, string filter = null)
        {
            if (!(GetDir(path) is DirNode dir))
                return Enumerable.Empty<string>();

            var regex = filter == null ? null : FilterToRegex(filter);
            return dir
                .Where(kvp => kvp.Value is FileNode && (regex?.IsMatch(kvp.Key) ?? true))
                .Select(x => x.Value.Path);
        }

        public Stream OpenRead(string path) =>
            GetFile(path) switch
            {
                ({ }, FileNode file) => new MockFileStream(file.Stream, true),
                ({ }, DirNode _) => throw new UnauthorizedAccessException($"Access to the path '{path}' is denied."),
                _ => throw new FileNotFoundException($"Could not find file '{path}'.")
            };

        public Stream OpenWriteTruncate(string path)
        {
            var filename = Path.GetFileName(path);
            switch (GetFile(path))
            {
                case ({ }, FileNode file):
                    file.Stream.Position = 0;
                    file.Stream.SetLength(0);
                    return new MockFileStream(file.Stream);
                case ({ }, DirNode _): throw new UnauthorizedAccessException($"Access to the path '{path}' is denied.");
                case ({ } dir, null):
                    var newFile = new FileNode($"{dir.Path}\\{filename}");
                    dir[filename] = newFile;
                    return new MockFileStream(newFile.Stream);
                default: throw new FileNotFoundException($"Could not find file '{path}'.");
            }
        }

        public void DeleteFile(string path)
        {
            var (dir, file) = GetFile(path);
            if (file == null)
                return;

            dir.Remove(Path.GetFileName(path));
        }

        public string ReadAllText(string path)
        {
            using var s = OpenRead(path);
            using var sr = new StreamReader(s);
            return sr.ReadToEnd();
        }

        public void WriteAllText(string path, string fullText)
        {
            using var s = OpenWriteTruncate(path);
            using var sw = new StreamWriter(s);
            sw.Write(fullText);
        }

        public IEnumerable<string> ReadAllLines(string path)
        {
            using var s = OpenRead(path);
            using var sr = new StreamReader(s);
            while (!sr.EndOfStream)
                yield return sr.ReadLine();
        }

        public byte[] ReadAllBytes(string path)
        {
            using var s = OpenRead(path);
            using var br = new BinaryReader(s);
            return br.ReadBytes((int)s.Length);
        }

        public void WriteAllBytes(string path, byte[] bytes)
        {
            using var s = OpenWriteTruncate(path);
            using var bw = new BinaryWriter(s);
            bw.Write(bytes);
        }
    }
}