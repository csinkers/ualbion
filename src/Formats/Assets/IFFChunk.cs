using System;
using SerdesNet;

namespace UAlbion.Formats.Assets;

public class IffChunk
{
    public string TypeId { get; private set; }
    public int Length { get; private set; }
    long _lengthOffset;

    IffChunk() { }
    public IffChunk(string typeId, int length)
    {
        TypeId = typeId;
        Length = length;
    }

    public void WriteLength(ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        var offset = s.Offset;
        s.Seek(_lengthOffset);
        Length = s.Int32BE(nameof(Length), (int)(offset - _lengthOffset));
        s.Seek(offset);
    }

    public static IffChunk Serdes(int _, IffChunk c, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        c ??= new IffChunk();
        c.TypeId = s.FixedLengthString(nameof(TypeId), c.TypeId, 4);
        c._lengthOffset = s.Offset;
        c.Length = s.Int32BE(nameof(Length), c.Length);
        return c;
    }

    public override string ToString() => $"{TypeId}: {Length} bytes";
}