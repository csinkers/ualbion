using System;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Veldrid.Textures;

public static class ImageSharpUtil
{
    public static Image<Rgba32> ToImageSharp(ReadOnlyImageBuffer<byte> from, ReadOnlySpan<uint> palette)
    {
        Image<Rgba32> image = new Image<Rgba32>(from.Width, from.Height);
        if (!image.TryGetSinglePixelSpan(out var rgbaSpan))
            throw new InvalidOperationException("Could not retrieve single span from Image");

        Span<uint> toBuffer = MemoryMarshal.Cast<Rgba32, uint>(rgbaSpan);
        var to = new ImageBuffer<uint>(from.Width, from.Height, from.Width, toBuffer);
        BlitUtil.BlitTiled8To32(from, to, palette, 255, 0);

        return image;
    }

    public static Image<Rgba32> ToImageSharp(ReadOnlyImageBuffer<uint> from)
    {
        Image<Rgba32> image = new Image<Rgba32>(from.Width, from.Height);
        if (!image.TryGetSinglePixelSpan(out var rgbaSpan))
            throw new InvalidOperationException("Could not retrieve single span from Image");

        Span<uint> toBuffer = MemoryMarshal.Cast<Rgba32, uint>(rgbaSpan);
        var to = new ImageBuffer<uint>(from.Width, from.Height, from.Width, toBuffer);
        BlitUtil.BlitDirect(from, to);
        return image;
    }

    public static Image<Rgba32> PackSpriteSheet(uint[] palette, int frameCount, GetFrameDelegate<byte> getFrame)
    {
        var layout = SpriteSheetUtil.ArrangeSpriteSheet(frameCount, 0, getFrame);
        if (layout.Layers > 1)
            throw new InvalidOperationException("Could not pack sprite sheet into a single layer");

        Image<Rgba32> image = new Image<Rgba32>(layout.Width, layout.Height);
        if (!image.TryGetSinglePixelSpan(out var rgbaSpan))
            throw new InvalidOperationException("Could not retrieve single span from Image");

        Span<uint> pixels = MemoryMarshal.Cast<Rgba32, uint>(rgbaSpan);
        for (var i = 0; i < frameCount; i++)
        {
            var frame = getFrame(i);
            var (toX, toY, toLayer) = layout.Positions[i];
            var target = pixels[(toX + toY * layout.Width)..];
            var to = new ImageBuffer<uint>(frame.Width, frame.Height, layout.Width, target);
            BlitUtil.BlitTiled8To32(frame, to, palette, 255, 0);
        }

        return image;
    }

    public static object FromImageSharp(IAssetId id, string name, Image<Rgba32> image)
    {
        if (image == null) throw new ArgumentNullException(nameof(image));
        image.TryGetSinglePixelSpan(out var pixels);
        var asUint = MemoryMarshal.Cast<Rgba32, uint>(pixels);
        var result = new SimpleTexture<uint>(id, name, image.Width, image.Height, asUint);
        result.AddRegion(0, 0, image.Width, image.Height);
        return result;
    }
}