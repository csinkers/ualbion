using System;
using System.IO;

namespace UAlbion.Core
{
    internal static class AssetHelper
    {
        static readonly string AssetRoot = Path.Combine(Environment.CurrentDirectory, "data");

        internal static string GetPath(string assetPath)
        {
            return Path.Combine(AssetRoot, assetPath);
        }
    }
}