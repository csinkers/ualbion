using System;

namespace UAlbion.Api.Visual;

[AttributeUsage(AttributeTargets.Field)]
public sealed class PixelFormatBytesAttribute : Attribute
{
    public int Bytes { get; }
    public PixelFormatBytesAttribute(int bytes) => Bytes = bytes;
}