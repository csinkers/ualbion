using System.Collections.Generic;
using System.IO;
using UAlbion.Api;

namespace UAlbion.Formats.Assets.Flic
{
    public class FullByteOrientedRleChunk : FlicChunk
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

        public IEnumerable<byte> ReadLinePixels(BinaryReader br)
        {
            var startOfLine = br.BaseStream.Position;
            int x = 0;
            while (x < _width)
            {
                sbyte type = br.ReadSByte();
                if (type >= 0)
                {
                    byte value = br.ReadByte();
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
                        yield return br.ReadByte();
                        x++;
                        type++;
                    }
                }

                if (x > _width)
                    ApiUtil.Assert("Overlength RLE line");
            }
        }

        protected override uint LoadChunk(uint length, BinaryReader br)
        {
            var start = br.BaseStream.Position;
            int i = 0;
            for (int y = 0; y < _height; y++)
            {
                byte _ = br.ReadByte(); // old packet count, no longer used
                foreach (var pixel in ReadLinePixels(br))
                    PixelData[i++] = pixel;
            }

            return length;
        }
    }
}
