using System;

namespace UAlbion.Config
{
    /// <summary>
    /// Applied to AssetType values. If an asset type is localised,
    /// then it should be flushed from any caches and reloaded when the
    /// game language is changed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LocalisedAttribute : Attribute { }
}