using System;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers;

public class SingleHeaderSpriteLoader : IAssetLoader<IReadOnlyTexture<byte>>
{
    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes((IReadOnlyTexture<byte>)existing, s, context);

    public IReadOnlyTexture<byte> Serdes(IReadOnlyTexture<byte> existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(s);
        ArgumentNullException.ThrowIfNull(context);

        if (s.IsWriting())
        {
            ArgumentNullException.ThrowIfNull(existing);
            Write(existing, s);
            return existing;
        }

        return Read(context, s);
    }

    static SimpleTexture<byte> Read(AssetLoadContext context, ISerdes s)
    {
        ushort width = s.UInt16("Width", 0);
        ushort height = s.UInt16("Height", 0);
        int something = s.UInt8(null, 0);
        ApiUtil.Assert(something == 0);
        byte frameCount = s.UInt8("Frames", 1);

        var result = new SimpleTexture<byte>(context.AssetId, width, height * frameCount);
        for (int i = 0; i < frameCount; i++)
        {
            byte[] frameBytes = s.Bytes("Frame" + i, null, width * height);
            result.AddRegion(0, i * height, width, height);
            BlitUtil.BlitDirect(
                new ReadOnlyImageBuffer<byte>(width, height, width, frameBytes),
                result.GetMutableRegionBuffer(i));
        }

        return result;
    }

    static void Write(IReadOnlyTexture<byte> existing, ISerdes s)
    {
        if (existing.Regions.Count == 0)
            throw new InvalidOperationException("Tried to write SingleHeader sprite without any regions");

        ushort width = (ushort)existing.Regions[0].Width;
        ushort height = (ushort)existing.Regions[0].Height;
        var frameCount = (byte)existing.Regions.Count;

        foreach (var region in existing.Regions)
        {
            if (region.Width == width && region.Height == height)
                continue;

            var distinctSizes = existing.Regions.Select(x => (x.Width, x.Height)).Distinct();
            var parts = distinctSizes.Select(x => $"({x.Width}, {x.Height})");
            var joined = string.Join(", ", parts);
            throw new InvalidOperationException($"Tried to a write an image with non-uniform frames to a single-header sprite (sizes: {joined})");
        }

        s.UInt16("Width", width);
        s.UInt16("Height", height);
        s.UInt8(null, 0);
        s.UInt8("Frames", frameCount);

        var frameBytes = new byte[width * height];
        var frame = new ImageBuffer<byte>(width, height, width, frameBytes);
        for (int i = 0; i < frameCount; i++)
        {
            BlitUtil.BlitDirect(existing.GetRegionBuffer(i), frame);
            s.Bytes("Frame" + i, frameBytes, width * height);
        }
    }
}
