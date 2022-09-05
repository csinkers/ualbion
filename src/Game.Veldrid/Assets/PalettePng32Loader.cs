using System;
using SerdesNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Veldrid.Assets;

public class PalettePng32Loader : Component, IAssetLoader<AlbionPalette>
{
    public AlbionPalette Serdes(AlbionPalette existing, AssetInfo info, ISerializer s, SerdesContext context)
    {
        if (existing == null) throw new ArgumentNullException(nameof(existing));
        if (s == null) throw new ArgumentNullException(nameof(s));

        if (!s.IsWriting())
            throw new NotSupportedException();

        var common = Resolve<IAssetManager>().LoadPalette(Base.Palette.Common);

        var pixels = new uint[256];
        BlitUtil.BlitDirect(
            new ReadOnlyImageBuffer<uint>(256, 1, 256, common.Texture.PixelData),
            new ImageBuffer<uint>(256, 1, 256, pixels));

        BlitUtil.BlitDirect(
            new ReadOnlyImageBuffer<uint>(192, 1, 192, existing.Texture.PixelData[..192]),
            new ImageBuffer<uint>(192, 1, 192, pixels));

        var buffer = new ReadOnlyImageBuffer<uint>(256, 1, 256, pixels);
        Image<Rgba32> image = ImageSharpUtil.ToImageSharp(buffer);

        var encoder = new PngEncoder();
        var bytes = FormatUtil.BytesFromStream(stream => encoder.Encode(image, stream));
        s.Bytes("PngBytes", bytes, bytes.Length);

        return existing;
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes((AlbionPalette)existing, info, s, context);
}