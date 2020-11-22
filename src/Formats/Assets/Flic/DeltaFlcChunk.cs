using System;
using SerdesNet;

namespace UAlbion.Formats.Assets.Flic
{
    public class DeltaFlcChunk : FlicChunk
    {
        DeltaFlcLine[] _lines;
        public override FlicChunkType Type => FlicChunkType.DeltaWordOrientedRle;

        protected override uint LoadChunk(uint length, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var start = s.Offset;
            ushort lineCount = s.UInt16(null, 0);
            _lines ??= new DeltaFlcLine[lineCount];
            for (int i = 0; i < lineCount; i++)
                _lines[i] = new DeltaFlcLine(s);
            return (ushort)(s.Offset - start);
        }

        public void Apply(byte[] buffer8, int width)
        {
            if (buffer8 == null) throw new ArgumentNullException(nameof(buffer8));
            int y = 0;
            int x = 0;
            void Write(byte b)
            {
                buffer8[y * width + x] = b;
                x++;
            }

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
                            Write((byte)(token.PixelData[i] & 0xff));
                            Write((byte)((token.PixelData[i] & 0xff00) >> 8));
                        }
                    }
                    else // RLE
                    {
                        for (int i = 0; i < -token.SignedCount; i++)
                        {
                            Write((byte)(token.PixelData[0] & 0xff));
                            Write((byte)((token.PixelData[0] & 0xff00) >> 8));
                        }
                    }
                }

                x = 0;
                y++;
            }
        }
    }
}
