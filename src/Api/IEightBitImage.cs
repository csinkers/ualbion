using System;

namespace UAlbion.Api
{
    public interface IEightBitImage : IImage
    {
        ReadOnlySpan<byte> PixelData { get; }
    }
}