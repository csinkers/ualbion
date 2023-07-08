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
using UAlbion.Config.Properties;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

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

    public IReadOnlyTexture<byte> Serdes(IReadOnlyTexture<byte> existing, ISerializer s, AssetLoadContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var assets = Resolve<IAssetManager>();
        var paletteId = context.GetProperty(AssetProps.Palette);
        var palette = context.AssetId.Type == AssetType.FontGfx 
            ? FontDefinition.ExportPalette 
            : assets.LoadPalette(new PaletteId(paletteId)).GetUnambiguousPalette();

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
            if (context.Width == 0)
            {
                throw new InvalidOperationException($"The asset {context.AssetId} ({context.Filename}) is set to use " +
                                                    "PngSheetLoader, but does not have its Width property set");
            }

            if (context.Height == 0)
            {
                throw new InvalidOperationException($"The asset {context.AssetId} ({context.Filename}) is set to use " +
                                                    "PngSheetLoader, but does not have its Height property set");
            }

            var decoder = new PngDecoder();
            var configuration = new Configuration();
            var bytes = s.Bytes(null, null, (int) s.BytesRemaining);
            using var stream = new MemoryStream(bytes);
            using var image = decoder.Decode<Rgba32>(configuration, stream);
            return Read(context.AssetId, palette, image, context.Width, context.Height);
        }
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((IReadOnlyTexture<byte>)existing, s, context);
}
