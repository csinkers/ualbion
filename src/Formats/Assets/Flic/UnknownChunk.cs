using System;
using SerdesNet;

namespace UAlbion.Formats.Assets.Flic;

public class UnknownChunk : FlicChunk
{
    public byte[] Bytes { get; private set; }
    public override FlicChunkType Type { get; }
    public UnknownChunk(FlicChunkType type) => Type = type;
    public override string ToString() => $"Unknown:{Type} ({Bytes.Length} bytes)";
    protected override uint LoadChunk(uint length, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        Bytes = s.Bytes(null, null, (int)length);
        return (uint)Bytes.Length;
    }
}