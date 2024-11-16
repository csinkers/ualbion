using System;
using SerdesNet;

namespace UAlbion.Formats.Assets.Flic;

public class CopyChunk : FlicChunk
{
    public byte[] PixelData { get; private set; }
    public override FlicChunkType Type => FlicChunkType.FullUncompressed;
    protected override uint LoadChunk(uint length, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        PixelData = s.Bytes(null, null, (int)length);
        return length;
    }
}