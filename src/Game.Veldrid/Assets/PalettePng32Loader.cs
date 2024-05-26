using System;
using SerdesNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Veldrid.Assets;

public class PalettePng32Loader : GameComponent, IAssetLoader<AlbionPalette>
{
    public AlbionPalette Serdes(AlbionPalette existing, ISerializer s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(s);

        if (!s.IsWriting())
            throw new NotSupportedException();

        var common = Assets.LoadPalette(Base.Palette.Common);

        var pixels = new uint[256];
        BlitUtil.BlitDirect(
            new ReadOnlyImageBuffer<uint>(256, 1, 256, common.Texture.PixelData),
            new ImageBuffer<uint>(256, 1, 256, pixels));

        BlitUtil.BlitDirect(
            new ReadOnlyImageBuffer<uint>(192, 1, 192, existing.Texture.PixelData[..192]),
            new ImageBuffer<uint>(192, 1, 192, pixels));

        var buffer = new ReadOnlyImageBuffer<uint>(256, 1, 256, pixels);
        using Image<Rgba32> image = ImageSharpUtil.ToImageSharp(buffer);

        var encoder = new PngEncoder();
        var bytes = FormatUtil.BytesFromStream(stream => encoder.Encode(image, stream));
        s.Bytes("PngBytes", bytes, bytes.Length);

        return existing;
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((AlbionPalette)existing, s, context);
}
