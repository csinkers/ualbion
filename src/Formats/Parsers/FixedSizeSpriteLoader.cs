using System;
using System.Buffers;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Config.Properties;

namespace UAlbion.Formats.Parsers;

public class FixedSizeSpriteLoader : IAssetLoader<IReadOnlyTexture<byte>>
{
    public const string TypeString = "UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats";
    public static readonly BoolAssetProperty TransposedProperty = new("Transposed"); // For various textures in the 3D world that are stored with rows/columns flipped
    public static readonly IntAssetProperty ExtraBytesProperty = new("ExtraBytes"); // Used to suppress assertions when loading original assets that have incorrect sizes

    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes((IReadOnlyTexture<byte>) existing, s, context);

    public IReadOnlyTexture<byte> Serdes(IReadOnlyTexture<byte> existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(s);
        ArgumentNullException.ThrowIfNull(context);
        return s.IsWriting()
            ? Write(existing, context, s)
            : Read(context, s);
    }

    static SimpleTexture<byte> Read(AssetLoadContext context, ISerdes s)
    {
        var streamLength = s.BytesRemaining;
        if (streamLength == 0)
            return null;

        var info = context.Node;
        int width = info.Width;
        int height = info.Height;

        if (width == 0) width = (int)Math.Sqrt(streamLength);

        int totalHeight = (int)streamLength / width;
        if (height == 0) height = totalHeight;

        int spriteCount = Math.Max(1, unchecked((int)(streamLength / (width * height))));
        height = (int)streamLength / (width * spriteCount);

        byte[] pixelData = s.Bytes(null, null, (int)streamLength);
        int expectedPixelCount = width * height * spriteCount;
        int extra = info.GetProperty(ExtraBytesProperty);

        ApiUtil.Assert((expectedPixelCount + extra) == (int)streamLength,
            $"Extra pixels found when loading fixed size sprite {context.AssetId} " +
            $"({streamLength} bytes for a {width}x{height}x{spriteCount} image, expected {expectedPixelCount}");

        var frames = new Region[spriteCount];
        for (int n = 0; n < spriteCount; n++)
            frames[n] = new Region(0, height * n, width, height, width, totalHeight, 0);

        var sprite = new SimpleTexture<byte>(
            context.AssetId,
            context.AssetId.ToString(),
            width,
            height * spriteCount,
            pixelData.AsSpan(0, expectedPixelCount), // May be less than the streamlength
            frames);

        return info.GetProperty(TransposedProperty) ? Transpose(sprite) : sprite;
    }

    static IReadOnlyTexture<byte> Write(IReadOnlyTexture<byte> existing, AssetLoadContext context, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(existing);

        var f = existing.Regions[0];
        foreach (var frame in existing.Regions)
        {
            ApiUtil.Assert(f.Width == frame.Width, "FixedSizeSpriteLoader tried to serialise sprite with non-uniform frames");
            ApiUtil.Assert(f.Height == frame.Height, "FixedSizeSpriteLoader tried to serialise sprite with non-uniform frames");
        }

        var sprite = context.GetProperty(TransposedProperty)
            ? Transpose(existing)
            : existing;

        InnerWrite(sprite, s);
        return existing;
    }

    static void InnerWrite(IReadOnlyTexture<byte> sprite, ISerdes s)
    {
        var f = sprite.Regions[0];
        int frameSize = f.Width * f.Height;
        byte[] pixelData = ArrayPool<byte>.Shared.Rent(frameSize);
        try
        {
            for (int i = 0; i < sprite.Regions.Count; i++)
            {
                BlitUtil.BlitDirect(
                    sprite.GetRegionBuffer(i),
                    new ImageBuffer<byte>(f.Width, f.Height, f.Width, pixelData));

                s.Bytes(null, pixelData, frameSize);
            }
        }
        finally { ArrayPool<byte>.Shared.Return(pixelData); }
    }

    static SimpleTexture<byte> Transpose(IReadOnlyTexture<byte> sprite)
    {
        var firstFrame = sprite.Regions[0];
        int width = firstFrame.Width;
        int height = firstFrame.Height;
        int spriteCount = sprite.Regions.Count;

        int rotatedFrameHeight = width;
        byte[] pixelData = new byte[spriteCount * width * height];
        var frames = new Region[spriteCount];
        for (int i = 0; i < spriteCount; i++)
        {
            var oldFrame = sprite.Regions[i];
            frames[i] = new Region(0, rotatedFrameHeight * i, height, rotatedFrameHeight, height, width * spriteCount, 0);

            ApiUtil.TransposeImage(width, height, // TODO: This should really take stride via ImageBuffers etc
                sprite.PixelData.Slice(oldFrame.PixelOffset, oldFrame.PixelLength),
                pixelData.AsSpan(frames[i].PixelOffset, frames[i].PixelLength));
        }
        return new SimpleTexture<byte>(sprite.Id, sprite.Name, height, width * spriteCount, pixelData, frames);
    }
}
