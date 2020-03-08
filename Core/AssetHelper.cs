using System;
using System.IO;

namespace UAlbion.Core
{
    public static class AssetHelper
    {
        static readonly string AssetRoot = Path.Combine(Environment.CurrentDirectory, "data");

        public static string GetPath(string assetPath)
        {
            return Path.Combine(AssetRoot, assetPath);
        }
    }
}