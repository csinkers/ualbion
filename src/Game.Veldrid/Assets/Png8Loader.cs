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
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats;
using UAlbion.Formats.Ids;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game.Veldrid.Assets;

public class Png8Loader : GameComponent, IAssetLoader<IReadOnlyTexture<byte>>
{
    public IReadOnlyTexture<byte> Serdes(IReadOnlyTexture<byte> existing, ISerializer s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(s);
        ArgumentNullException.ThrowIfNull(context);

        var paletteId = context.PaletteId;

        if (paletteId.IsNone)
            throw new InvalidOperationException($"No palette id specified for {context.AssetId} ({context.AssetId.Id})");

        var palette = Assets.LoadPalette(paletteId);
        if (palette == null)
            throw new InvalidOperationException($"Could not load palette {paletteId} for asset {context.AssetId} in file {context.Filename}");

        var unambiguousPalette = palette.GetUnambiguousPalette();

        if (s.IsWriting())
        {
            ArgumentNullException.ThrowIfNull(existing);

            var encoder = new PngEncoder();
            PackedChunks.Pack(s, existing.Regions.Count, frameNum => Write(encoder, unambiguousPalette, existing, frameNum));
            return existing;
        }

        // Read
        var pngOptions = new PngDecoderOptions();
        var images = new List<Image<Rgba32>>();
        try
        {
            foreach (var (bytes, _) in PackedChunks.Unpack(s))
            {
                using var stream = new MemoryStream(bytes);
                images.Add(PngDecoder.Instance.Decode<Rgba32>(pngOptions, stream));
            }

            return Read(context.AssetId, unambiguousPalette, images, paletteId);
        }
        finally { foreach (var image in images) image.Dispose(); }
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((IReadOnlyTexture<byte>)existing, s, context);

    static byte[] Write(PngEncoder encoder, uint[] palette, IReadOnlyTexture<byte> existing, int frameNum)
    {
        var frame = existing.Regions[frameNum];
        var buffer = new ReadOnlyImageBuffer<byte>(
            frame.Width,
            frame.Height,
            existing.Width,
            existing.PixelData.Slice(frame.PixelOffset, frame.PixelLength));

        using Image<Rgba32> image = ImageSharpUtil.ToImageSharp(buffer, palette);
        var bytes = FormatUtil.BytesFromStream(stream => encoder.Encode(image, stream));
        return bytes;
    }

    static SimpleTexture<byte> Read(AssetId id, uint[] palette, IList<Image<Rgba32>> images, PaletteId paletteId)
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
            if (!image.DangerousTryGetSinglePixelMemory(out var rgbaMemory))
                throw new InvalidOperationException("Could not retrieve single span from Image");

            frames.Add(new Region(0, currentY, image.Width, image.Height, totalWidth, totalHeight, 0));
            var uintSpan = MemoryMarshal.Cast<Rgba32, uint>(rgbaMemory.Span);
            var from = new ReadOnlyImageBuffer<uint>(image.Width, image.Height, image.Width, uintSpan);
            var byteSpan = pixels.AsSpan(currentY * totalWidth, totalWidth * (image.Height - 1) + image.Width);
            var to = new ImageBuffer<byte>(image.Width, image.Height, totalWidth, byteSpan);
            BlitUtil.Blit32To8(from, to, palette, quantizeCache);

            currentY += image.Height;
        }

        return new SimpleTexture<byte>(id, id.ToString(), totalWidth, totalHeight, pixels, frames);
    }
}
