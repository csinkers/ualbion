using System;
using System.IO;
using System.Reflection;

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
    }
}