using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.Assets.Flic;

class FullByteOrientedRleChunk : FlicChunk
{
    readonly int _width;
    readonly int _height;
    public byte[] PixelData { get; }

    public FullByteOrientedRleChunk(int width, int height)
    {
        _width = width;
        _height = height;
        PixelData = new byte[width * height];
    }

    public override FlicChunkType Type => FlicChunkType.FullByteOrientedRle;

    public IEnumerable<byte> ReadLinePixels(ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        var startOfLine = s.Offset;
        int x = 0;
        while (x < _width)
        {
            sbyte type = s.Int8(null, 0);
            if (type >= 0)
            {
                byte value = s.UInt8(null, 0);
                while (type != 0)
                {
                    yield return value;
                    x++;
                    type--;
                }
            }
            else
            {
                while(type != 0)
                {
                    yield return s.UInt8(null, 0);
                    x++;
                    type++;
                }
            }

            if (x > _width)
                ApiUtil.Assert("Overlength RLE line");
        }
    }

    protected override uint LoadChunk(uint length, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        var start = s.Offset;
        int i = 0;
        for (int y = 0; y < _height; y++)
        {
            byte _ = s.UInt8(null, 0); // old packet count, no longer used
            foreach (var pixel in ReadLinePixels(s))
                PixelData[i++] = pixel;
        }

        return length;
    }
}