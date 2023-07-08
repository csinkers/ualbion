using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SerdesNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats;
using UAlbion.Formats.Ids;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game.Veldrid.Assets;

public class Png8Loader : Component, IAssetLoader<IReadOnlyTexture<byte>>
{
    static byte[] Write(IImageEncoder encoder, uint[] palette, IReadOnlyTexture<byte> existing, int frameNum)
    {
        var frame = existing.Regions[frameNum];
        var buffer = new ReadOnlyImageBuffer<byte>(
            frame.Width,
            frame.Height,
            existing.Width,
            existing.PixelData.Slice(frame.PixelOffset, frame.PixelLength));

        Image<Rgba32> image = ImageSharpUtil.ToImageSharp(buffer, palette);
        var bytes = FormatUtil.BytesFromStream(stream => encoder.Encode(image, stream));
        return bytes;
    }

    static IReadOnlyTexture<byte> Read(AssetId id, uint[] palette, IList<Image<Rgba32>> images)
    {
        int totalWidth = images.Max(x => x.Width);
        int totalHeight = images.Sum(x => x.Height);
        var pixels = new byte[totalWidth * totalHeight];
        var frames = new List<Region>();
        int currentY = 0;
        var quantizeCache = new Dictionary<uint, byte>();
        for (int i = 0; i < images.Count; i++)
        {
            Image<Rgba32> image = images[i];
            if (!image.TryGetSinglePixelSpan(out Span<Rgba32> rgbaSpan))
                throw new InvalidOperationException("Could not retrieve single span from Image");

            frames.Add(new Region(0, currentY, image.Width, image.Height, totalWidth, totalHeight, 0));
            var uintSpan = MemoryMarshal.Cast<Rgba32, uint>(rgbaSpan);
            var from = new ReadOnlyImageBuffer<uint>(image.Width, image.Height, image.Width, uintSpan);
            var byteSpan = pixels.AsSpan(currentY * totalWidth, totalWidth * (image.Height - 1) + image.Width);
            var to = new ImageBuffer<byte>(image.Width, image.Height, totalWidth, byteSpan);
            BlitUtil.Blit32To8(from, to, palette, quantizeCache);

            currentY += image.Height;
        }

        return new SimpleTexture<byte>(id, id.ToString(), totalWidth, totalHeight, pixels, frames);
    }

    public IReadOnlyTexture<byte> Serdes(IReadOnlyTexture<byte> existing, ISerializer s, AssetLoadContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var assets = Resolve<IAssetManager>();
        var paletteNum = context.PaletteId;
        var paletteId = new PaletteId(paletteNum);
        var palette = assets.LoadPalette(paletteId);
        if (palette == null)
            throw new InvalidOperationException($"Could not load palette {paletteId} ({paletteNum}) for asset {context.AssetId} in file {context.Filename}");

        var unambiguousPalette = palette.GetUnambiguousPalette();

        if (s.IsWriting())
        {
            if (existing == null)
                throw new ArgumentNullException(nameof(existing));

            var encoder = new PngEncoder();
            PackedChunks.Pack(s, existing.Regions.Count, frameNum => Write(encoder, unambiguousPalette, existing, frameNum));
            return existing;
        }

        // Read
        var decoder = new PngDecoder();
        var configuration = new Configuration();
        var images = new List<Image<Rgba32>>();
        try
        {
            foreach (var (bytes, _) in PackedChunks.Unpack(s))
            {
                using var stream = new MemoryStream(bytes);
                images.Add(decoder.Decode<Rgba32>(configuration, stream));
            }

            return Read(context.AssetId, unambiguousPalette, images);
        }
        finally { foreach (var image in images) image.Dispose(); }
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((IReadOnlyTexture<byte>)existing, s, context);
}
