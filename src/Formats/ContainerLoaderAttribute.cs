using System;
using UAlbion.Config;

namespace UAlbion.Formats
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ContainerLoaderAttribute : Attribute
    {
        public ContainerFormat[] SupportedFormats { get; }
        public ContainerLoaderAttribute(params ContainerFormat[] supportedFormats)
        {
            SupportedFormats = supportedFormats;
        }
    }
}