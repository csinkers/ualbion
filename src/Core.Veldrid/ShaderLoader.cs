using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid;

public sealed class ShaderLoader : ServiceComponent<IShaderLoader>, IShaderLoader, IDisposable
{
    static readonly Regex IncludeRegex = new("^#include\\s+\"([^\"]+)\"");
    readonly List<FileSystemWatcher> _watchers = new();
    readonly List<string> _directories = new();

    public event EventHandler<EventArgs> ShadersUpdated;
    public void AddShaderDirectory(string directory)
    {
        var watcher = new FileSystemWatcher(directory);
        watcher.Changed += (sender, _) => ShadersUpdated?.Invoke(sender, EventArgs.Empty);

        watcher.EnableRaisingEvents = true;
        _watchers.Add(watcher);
        _directories.Add(directory);
    }

    public ShaderInfo Load(string path, IFileSystem disk)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        if (disk == null) throw new ArgumentNullException(nameof(disk));

        foreach (var dir in _directories)
        {
            var candidate = Path.Combine(dir, path);
            if (disk.FileExists(candidate))
                return LoadInner(candidate, disk);
        }

        throw new FileNotFoundException($"Could not find shader \"{path}\" in any loaded mod's ShaderPath directory");
    }

    static ShaderInfo LoadInner(string path, IFileSystem disk)
    {
        var name = Path.GetFileName(path);
        var directory = Path.GetDirectoryName(path);
        var lines = disk.ReadAllLines(path);

        // Substitute include files
        var sb = new StringBuilder();

        bool first = true;
        foreach (var line in lines)
        {
            if (first)
            {
                if (!line.StartsWith("#version", StringComparison.Ordinal))
                {
                    sb.AppendLine("#version 450");
                }
#if DEBUG
                sb.AppendLine("#define DEBUG");
#endif
                first = false;
            }

            var match = IncludeRegex.Match(line);
            if (match.Success)
            {
                var relative = match.Groups[1].Value;
                var absPath = string.IsNullOrEmpty(directory) ? relative : Path.Combine(directory, relative);
                var includedContent = disk.ReadAllText(absPath);
                sb.AppendLine(includedContent);
            }
            else sb.AppendLine(line);
        }

        return new ShaderInfo(name, sb.ToString());
    }

    public void ClearDirectories()
    {
        foreach (var watcher in _watchers)
            watcher.Dispose();
        _watchers.Clear();
        _directories.Clear();
    }

    public void Dispose() => ClearDirectories();
}
