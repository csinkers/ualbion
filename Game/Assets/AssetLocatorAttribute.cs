using System;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Assets
{
    public class AssetLocatorAttribute : Attribute
    {
        public AssetType[] SupportedTypes { get; }

        public AssetLocatorAttribute(params AssetType[] supportedTypes)
        {
            SupportedTypes = supportedTypes;
        }
    }
}