using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers;

public class MultiHeaderSpriteLoader : IAssetLoader<IReadOnlyTexture<byte>>
{
    public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        => Serdes((IReadOnlyTexture<byte>)existing, info, mapping, s, jsonUtil);

    public IReadOnlyTexture<byte> Serdes(IReadOnlyTexture<byte> existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (s.IsWriting())
        {
            if (existing == null)
                throw new ArgumentNullException(nameof(existing));

            Write(existing, s);
            return existing;
        }

        return Read(info, s);
    }

    static IReadOnlyTexture<byte> Read(AssetInfo info, ISerializer s)
    {
        int width = s.UInt16("Width", 0);
        int height = s.UInt16("Height", 0);
        int something = s.UInt8(null, 0);
        ApiUtil.Assert(something == 0);
        byte frameCount = s.UInt8("Frames", 1);

        // TODO: When writing, assert that uniform and the frame sizes match up
        var frames = new List<IReadOnlyTexture<byte>>();
        for (int i = 0; i < frameCount; i++)
        {
            if (i > 0)
            {
                width = s.UInt16("FrameWidth", 0);
                height = s.UInt16("FrameHeight", 0);
                something = s.UInt8(null, 0);
                ApiUtil.Assert(something == 0);
                byte spriteCount2 = s.UInt8(null, frameCount);
                ApiUtil.Assert(spriteCount2 == frameCount);
            }

            byte[] frameBytes = s.Bytes("Frame" + i, null, width * height);
            frames.Add(new SimpleTexture<byte>(null, null, width, height, frameBytes));
        }

        s.Check();

        return BlitUtil.CombineFramesVertically<byte>(info.AssetId, frames);
    }

    static void Write(IReadOnlyTexture<byte> existing, ISerializer s)
    {
        if (existing.Regions.Count > 255)
            throw new ArgumentOutOfRangeException($"Tried to save an image with more than 255 frames as a multi-header sprite ({existing.Name} with {existing.Regions.Count} regions)");

        for (int i = 0; i < existing.Regions.Count; i++)
        {
            var region = existing.Regions[i];
            s.UInt16("FrameWidth", (ushort)region.Width);
            s.UInt16("FrameHeight", (ushort)region.Height);
            s.UInt8(null, 0);
            s.UInt8(null, (byte)existing.Regions.Count);

            var frameSize = region.Width * region.Height;
            byte[] frameBytes = ArrayPool<byte>.Shared.Rent(frameSize);
            try
            {
                Debug.Assert(existing != null, nameof(existing) + " != null");

                BlitUtil.BlitDirect(
                    existing.GetRegionBuffer(i),
                    new ImageBuffer<byte>(region.Width, region.Height, region.Width, frameBytes));

                s.Bytes("Frame" + i, frameBytes, region.Width * region.Height);
            }
            finally { ArrayPool<byte>.Shared.Return(frameBytes); }
        }
    }
}