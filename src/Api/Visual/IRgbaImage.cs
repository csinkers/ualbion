using System;

namespace UAlbion.Api.Visual
{
    public interface IRgbaImage : IImage
    {
        ReadOnlySpan<uint> PixelData { get; }
    }
}