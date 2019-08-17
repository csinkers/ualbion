using System;

namespace UAlbion.Formats
{
    public class AssetLoaderAttribute : Attribute
    {
        public XldObjectType[] SupportedTypes { get; }
        public AssetLoaderAttribute(params XldObjectType[] supportedTypes)
        {
            SupportedTypes = supportedTypes;
        }
    }
}