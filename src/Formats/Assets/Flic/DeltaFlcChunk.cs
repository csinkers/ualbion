using System;
using SerdesNet;

namespace UAlbion.Formats.Assets.Flic;

public class DeltaFlcChunk : FlicChunk
{
    DeltaFlcLine[] _lines;
    public override FlicChunkType Type => FlicChunkType.DeltaWordOrientedRle;

    protected override uint LoadChunk(uint length, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        var start = s.Offset;
        ushort lineCount = s.UInt16(null, 0);
        _lines ??= new DeltaFlcLine[lineCount];
        for (int i = 0; i < lineCount; i++)
            _lines[i] = new DeltaFlcLine(s);
        return (ushort)(s.Offset - start);
    }

    public void Apply(Span<byte> buffer8, int width)
    {
        int y = 0;
        int x = 0;

        foreach (var line in _lines)
        {
            y += line.Skip;
            foreach (var token in line.Tokens)
            {
                x += token.ColumnSkipCount;
                if (token.SignedCount > 0)
                {
                    for (int i = 0; i < token.SignedCount; i++)
                    {
                        buffer8[y * width + x] = (byte)(token.PixelData[i] & 0xff);
                        x++;
                        buffer8[y * width + x] = (byte)((token.PixelData[i] & 0xff00) >> 8);
                        x++;
                    }
                }
                else // RLE
                {
                    for (int i = 0; i < -token.SignedCount; i++)
                    {
                        buffer8[y * width + x] = (byte)(token.PixelData[0] & 0xff);
                        x++;
                        buffer8[y * width + x] = (byte)((token.PixelData[0] & 0xff00) >> 8);
                        x++;
                    }
                }
            }

            x = 0;
            y++;
        }
    }
}