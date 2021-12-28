using System;
using SerdesNet;

namespace UAlbion.Formats.Assets.Flic;

public class CopyChunk : FlicChunk
{
    public byte[] PixelData { get; private set; }
    public override FlicChunkType Type => FlicChunkType.FullUncompressed;
    protected override uint LoadChunk(uint length, ISerializer br)
    {
        if (br == null) throw new ArgumentNullException(nameof(br));
        PixelData = br.Bytes(null, null, (int)length);
        return length;
    }
}