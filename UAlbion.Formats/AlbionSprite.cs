using System.Diagnostics;
using System.IO;

namespace UAlbion.Formats
{
    public class AlbionFrame
    {
        public AlbionFrame(int width, int height, byte[] bytes)
        {
            Width = width;
            Height = height;
            Pixels = bytes;
        }

        public int Width { get; }
        public int Height { get; }
        public byte[] Pixels { get; }
    }

    public class AlbionSprite
    {
        public string Name { get; }
        public AlbionFrame[] Frames { get; }

        public AlbionSprite(BinaryReader br, long streamLength, int width, int height, string name)
        {
            Debug.Assert(streamLength % width == 0);
            Debug.Assert(streamLength % (width*height) == 0);
            Name = name;
            long initialPosition = br.BaseStream.Position;

            int spriteCount = unchecked((int)(streamLength / (width * height)));
            Frames = new AlbionFrame[spriteCount];
            for (int i = 0; i < spriteCount; i++)
            {
                var bytes = br.ReadBytes(width * height);
                Frames[i] = new AlbionFrame(width, height, bytes);
            }
            Debug.Assert(br.BaseStream.Position == initialPosition + streamLength);
        }

        public AlbionSprite(BinaryReader br, long streamLength, string name)
        {
            Name = name;
            long initialPosition = br.BaseStream.Position;

            int width = br.ReadUInt16();
            int height = br.ReadUInt16();
            int spriteCount = br.ReadUInt16();
            Frames = new AlbionFrame[spriteCount];
            for (int i = 0; i < spriteCount; i++)
            {
                var bytes = br.ReadBytes(width * height);
                Frames[i] = new AlbionFrame(width, height, bytes);
            }

            Debug.Assert(br.BaseStream.Position == initialPosition + streamLength);
        }
    }
}
