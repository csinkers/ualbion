using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SerdesNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Veldrid.Assets;

public class PngSheetLoader : Component, IAssetLoader<IReadOnlyTexture<byte>> // For fonts etc
{
    static byte[] Write(IImageEncoder encoder, uint[] palette, IReadOnlyTexture<byte> existing)
    {
        var image = ImageSharpUtil.PackSpriteSheet(palette, existing.Regions.Count, existing.GetRegionBuffer);
        return FormatUtil.BytesFromStream(stream => encoder.Encode(image, stream));
    }

    static IReadOnlyTexture<byte> Read(AssetId id, uint[] palette, Image<Rgba32> image, int subItemWidth, int subItemHeight)
    {
        var pixels = new byte[image.Width * image.Height];
        var frames = new List<Region>();
        if (!image.TryGetSinglePixelSpan(out Span<Rgba32> rgbaSpan))
            throw new InvalidOperationException("Could not retrieve single span from Image");

        var uintSpan = MemoryMarshal.Cast<Rgba32, uint>(rgbaSpan);
        var source = new ReadOnlyImageBuffer<uint>(image.Width, image.Height, image.Width, uintSpan);
        var dest = new ImageBuffer<byte>(image.Width, image.Height, image.Width, pixels);
        BlitUtil.UnpackSpriteSheet(palette, subItemWidth, subItemHeight, source, dest,
            (x,y,w,h) => frames.Add(new Region(x, y, w, h, image.Width, image.Height, 0)));

        while (IsFrameEmpty(frames.Last(), pixels, image.Width))
            frames.RemoveAt(frames.Count - 1);

        return new SimpleTexture<byte>(id, id.ToString(), image.Width, image.Height, pixels, frames);
    }

    static bool IsFrameEmpty(Region frame, byte[] pixels, int stride)
    {
        var span = pixels.AsSpan(frame.PixelOffset, frame.PixelLength);
        for (int j = 0; j < frame.Height; j++)
        for (int i = 0; i < frame.Width; i++)
            if (span[i + j * stride] != 0)
                return false;
        return true;
    }

    public IReadOnlyTexture<byte> Serdes(IReadOnlyTexture<byte> existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var paletteId = info.Get(AssetProperty.PaletteId, 0);
        var palette = context.Assets
            .LoadPalette(new PaletteId(AssetType.Palette, paletteId))
            .GetUnambiguousPalette();

        if (info.AssetId.Type == AssetType.Font)
        {
            palette = new uint[256];
            palette[1] = 0xffffffff;
            palette[2] = 0xffcccccc;
            palette[3] = 0xffaaaaaa;
            palette[4] = 0xff777777;
            palette[5] = 0xff555555;
        }

        if (s.IsWriting())
        {
            if (existing == null)
                throw new ArgumentNullException(nameof(existing));
            var encoder = new PngEncoder();
            var bytes = Write(encoder, palette, existing);
            s.Bytes(null, bytes, bytes.Length);
            return existing;
        }
        else // Read
        {
            var decoder = new PngDecoder();
            var configuration = new Configuration();
            var bytes = s.Bytes(null, null, (int) s.BytesRemaining);
            using var stream = new MemoryStream(bytes);
            using var image = decoder.Decode<Rgba32>(configuration, stream);
            return Read(info.AssetId, palette, image, info.Width, info.Height);
        }
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes((IReadOnlyTexture<byte>)existing, info, s, context);
}
