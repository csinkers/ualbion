using System;

namespace UAlbion.Config
{
    /// <summary>
    /// If an AssetType is isomorphic to another then it will have the
    /// same number of values, the same asset names and AssetIds of
    /// one type can be converted to the other by simply changing
    /// the AssetType / most significant byte. Helper methods are also created by the code generator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class IsomorphicToAttribute : Attribute
    {
        public AssetType Type { get; }
        public IsomorphicToAttribute(AssetType type) => Type = type;
    }
}