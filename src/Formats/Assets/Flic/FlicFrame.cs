using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.Assets.Flic;

public class FlicFrame : FlicChunk
{
    readonly int _videoWidth;
    readonly int _videoHeight;

    public FlicFrame(int width, int height)
    {
        _videoWidth = width;
        _videoHeight = height;
    }

    public override FlicChunkType Type => FlicChunkType.Frame;
    public IList<FlicChunk> SubChunks { get; } = new List<FlicChunk>();
    public ushort Delay { get; private set; }
    public ushort Width { get; private set; } // Overrides, usually 0.
    public ushort Height { get; private set; }

    protected override uint LoadChunk(uint length, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);
        var initialOffset = s.Offset;
        ushort subChunkCount = s.UInt16(null, 0);
        Delay = s.UInt16(null, 0);
        s.UInt16(null, 0);
        Width = s.UInt16(null, 0);
        Height = s.UInt16(null, 0);

        for (int i = 0; i < subChunkCount; i++)
            SubChunks.Add(Load(s, _videoWidth, _videoHeight));

        ApiUtil.Assert(s.Offset == initialOffset + length);
        return length;
    }
}