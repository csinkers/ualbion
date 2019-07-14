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

        int Width { get; }
        int Height { get; }
        byte[] Pixels { get; }
    }

    public class AlbionSprite
    {
        readonly AlbionFrame[] _sprites;

        public AlbionSprite(BinaryReader br, long streamLength, int width, int height)
        {
            Debug.Assert(streamLength % width == 0);
            Debug.Assert(streamLength % (width*height) == 0);
            long initialPosition = br.BaseStream.Position;

            int spriteCount = unchecked((int)(streamLength / (width * height)));
            _sprites = new AlbionFrame[spriteCount];
            for (int i = 0; i < spriteCount; i++)
            {
                var bytes = br.ReadBytes(width * height);
                _sprites[i] = new AlbionFrame(width, height, bytes);
            }
            Debug.Assert(br.BaseStream.Position == initialPosition + streamLength);
        }

        public AlbionSprite(BinaryReader br, long streamLength, AssetType type)
        {
            if(type == AssetType.Picture)
            {
                // Load bitmap
            }
            else
            {
                long initialPosition = br.BaseStream.Position;

                int width = br.ReadUInt16();
                int height = br.ReadUInt16();
                int spriteCount = br.ReadUInt16();
                _sprites = new AlbionFrame[spriteCount];
                for (int i = 0; i < spriteCount; i++)
                {
                    var bytes = br.ReadBytes(width * height);
                    _sprites[i] = new AlbionFrame(width, height, bytes);
                }

                Debug.Assert(br.BaseStream.Position == initialPosition + streamLength);
            }
        }
    }
}
