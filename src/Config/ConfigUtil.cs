using System;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UAlbion.Api;

namespace UAlbion.Config
{
    public static class ConfigUtil
    {
        public static JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            ContractResolver = new PrivatePropertyJsonContractResolver()
        };

        public static string FindBasePath(IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            var exeLocation = Assembly.GetExecutingAssembly().Location;
            var curDir = new DirectoryInfo(Path.GetDirectoryName(exeLocation) ?? throw new InvalidOperationException());
            while (curDir != null && !disk.FileExists(Path.Combine(curDir.FullName, "data", "config.json")))
                curDir = curDir.Parent;

            var baseDir = curDir?.FullName;
            return baseDir;
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
            foreach (var _ in curDir.Substring(i).Split(OneSlash, StringSplitOptions.RemoveEmptyEntries))
                sb.Append("../");

            sb.Append(path.Substring(i).Trim('/'));
            var result = sb.ToString();
            if (result.Length == 0)
                result = ".";
            return useForwardSlash ? result : result.Replace('/', '\\');
        }

        public static string AssetName(AssetId id)
        {
            var full = id.ToString();
            int index = full.IndexOf('.');
            return index == -1 ? full : full.Substring(index + 1);
        }
    }
}
