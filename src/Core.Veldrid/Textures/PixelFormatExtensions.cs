using System;
using Veldrid;

namespace UAlbion.Core.Veldrid.Textures
{
    public static class PixelFormatExtensions
    {
        public static PixelFormat ToVeldrid(this Core.Textures.PixelFormat format) => format switch
        {
            Core.Textures.PixelFormat.EightBit => PixelFormat.R8_UNorm,
            Core.Textures.PixelFormat.Rgba32 => PixelFormat.R8_G8_B8_A8_UNorm,
            _ => throw new ArgumentOutOfRangeException(
                nameof(format),
                format,
                $"Pixel format {format} needs to be added to the UAlbion.Core.Veldrid.Texture.PixelFormatExtensions.ToVeldrid switch statement.")
        };
    }
}
