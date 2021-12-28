using System;

namespace UAlbion.Api.Visual;

[AttributeUsage(AttributeTargets.Field)]
public class PixelFormatBytesAttribute : Attribute
{
    public int Bytes { get; }
    public PixelFormatBytesAttribute(int bytes) => Bytes = bytes;
}