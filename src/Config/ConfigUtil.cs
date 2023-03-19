using System;
using System.IO;
using System.Reflection;
using System.Text;
using UAlbion.Api;

namespace UAlbion.Config;

public static class ConfigUtil
{
    static readonly string ProbePath = Path.Combine("mods", "Base", "base_assets.json");
    public static string FindBasePath(IFileSystem disk) // Should roughly match UAlbion.Scripting.Tests.TestUtil
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));

        static string Probe(string start, IFileSystem disk)
        {
            var curDir = new DirectoryInfo(start ?? throw new InvalidOperationException());

            while (curDir != null && !disk.FileExists(Path.Combine(curDir.FullName, ProbePath)))
                curDir = curDir.Parent;

            return curDir?.FullName;
        }

        var exeLocation = Assembly.GetExecutingAssembly().Location;
        var result = Probe(Path.GetDirectoryName(exeLocation), disk) ?? Probe(disk.CurrentDirectory, disk);
        return result;
    }

    static readonly char[] OneSlash = { '/' };
    public static string GetRelativePath(string path, string curDir, bool useForwardSlash)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        if (curDir == null) throw new ArgumentNullException(nameof(curDir));

        path = path.Replace('\\', '/');
        curDir = curDir.Replace('\\', '/');
        int i;
        for (i = 0; i < path.Length && i < curDir.Length; i++)
            if (path[i] != curDir[i]) // Find common substring
                break;

        var sb = new StringBuilder();
        foreach (var _ in curDir[i..].Split(OneSlash, StringSplitOptions.RemoveEmptyEntries))
            sb.Append("../");

        sb.Append(path[i..].Trim('/'));
        var result = sb.ToString();
        if (result.Length == 0)
            result = ".";
        return useForwardSlash ? result : result.Replace('/', '\\');
    }

    public static string AssetName(AssetId id)
    {
        var full = id.ToString();
        int index = full.IndexOf('.', StringComparison.Ordinal);
        return index == -1 ? full : full[(index + 1)..];
    }
}
