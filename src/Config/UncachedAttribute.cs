using System;

namespace UAlbion.Config
{
    /// <summary>
    /// Applied to AssetType values. If an asset type is uncached,
    /// then it should be reloaded from disk every time.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class UncachedAttribute : Attribute { }
}