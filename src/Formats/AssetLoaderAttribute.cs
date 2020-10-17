using System;
using UAlbion.Config;

namespace UAlbion.Formats
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AssetLoaderAttribute : Attribute
    {
        public FileFormat[] SupportedFormats { get; }
        public AssetLoaderAttribute(params FileFormat[] supportedFormats)
        {
            SupportedFormats = supportedFormats;
        }
    }
}
