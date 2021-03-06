using System;

namespace UAlbion.Api
{
    public interface IRgbaImage : IImage
    {
        ReadOnlySpan<uint> PixelData { get; }
    }
}