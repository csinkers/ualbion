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
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game.Veldrid.Assets;

public class Png32Loader : Component, IAssetLoader<IReadOnlyTexture<uint>>
{
    static byte[] Write(IImageEncoder encoder, IReadOnlyTexture<uint> existing, int frameNum)
    {
        var frame = existing.Regions[frameNum];
        var buffer = new ReadOnlyImageBuffer<uint>(
            frame.Width,
            frame.Height,
            existing.Width,
            existing.PixelData.Slice(frame.PixelOffset, frame.PixelLength));

        Image<Rgba32> image = ImageSharpUtil.ToImageSharp(buffer);
        return FormatUtil.BytesFromStream(stream => encoder.Encode(image, stream));
    }

    static IReadOnlyTexture<uint> Read(AssetId id, IList<Image<Rgba32>> images)
    {
        int totalWidth = images.Max(x => x.Width);
        int totalHeight = images.Sum(x => x.Height);
        var pixels = new uint[totalWidth * totalHeight];
        var frames = new List<Region>();
        int currentY = 0;
        for (int i = 0; i < images.Count; i++)
        {
            Image<Rgba32> image = images[i];
            if (!image.TryGetSinglePixelSpan(out Span<Rgba32> rgbaSpan))
                throw new InvalidOperationException("Could not retrieve single span from Image");

            frames.Add(new Region(0, currentY, image.Width, image.Height, totalWidth, totalHeight, 0));
            var fromSpan = MemoryMarshal.Cast<Rgba32, uint>(rgbaSpan);
            var from = new ReadOnlyImageBuffer<uint>(image.Width, image.Height, image.Width, fromSpan);
            var toSpan = pixels.AsSpan(currentY * totalWidth, totalWidth * (image.Height - 1) + image.Width);
            var to = new ImageBuffer<uint>(image.Width, image.Height, totalWidth, toSpan);
            BlitUtil.BlitDirect(from, to);

            currentY += image.Height;
        }

        return new SimpleTexture<uint>(id, id.ToString(), totalWidth, totalHeight, pixels, frames);
    }

    public IReadOnlyTexture<uint> Serdes(IReadOnlyTexture<uint> existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (s.IsWriting())
        {
            if (existing == null)
                throw new ArgumentNullException(nameof(existing));

            var encoder = new PngEncoder();
            PackedChunks.Pack(s, existing.Regions.Count, frameNum => Write(encoder, existing, frameNum));
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

            return Read(info.AssetId, images);
        }
        finally { foreach (var image in images) image.Dispose(); }
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes((IReadOnlyTexture<uint>)existing, info, s, context);
}