using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace UAlbion.Config
{
    public static class ConfigUtil
    {
        public static string FindBasePath()
        {
            var exeLocation = Assembly.GetExecutingAssembly().Location;
            var curDir = new DirectoryInfo(Path.GetDirectoryName(exeLocation) ?? throw new InvalidOperationException());
            while (curDir != null && !File.Exists(Path.Combine(curDir.FullName, "data", "config.json")))
                curDir = curDir.Parent;

            var baseDir = curDir?.FullName;
            return baseDir;
        }

        static readonly char[] OneSlash = { '/'};
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
    }
}