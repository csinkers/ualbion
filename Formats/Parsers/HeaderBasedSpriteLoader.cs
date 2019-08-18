using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(XldObjectType.SingleHeaderSprite, XldObjectType.HeaderPerSubImageSprite)]
    public class HeaderBasedSpriteLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            Debug.Assert(config.Parent.RotatedLeft != true);
            var sprite = new AlbionSprite();
            long initialPosition = br.BaseStream.Position;
            sprite.Name = name;
            sprite.UniformFrames = config.Type == XldObjectType.SingleHeaderSprite;

            int width = br.ReadUInt16();
            int height = br.ReadUInt16();
            int something = br.ReadByte();
            Debug.Assert(something == 0);
            int spriteCount = br.ReadByte();

            sprite.Frames = new AlbionSprite.Frame[spriteCount];
            var frameBytes = new List<byte[]>();
            int currentY = 0;

            sprite.Width = 0;
            for (int i = 0; i < spriteCount; i++)
            {
                if (!sprite.UniformFrames && i > 0)
                {
                    width = br.ReadUInt16();
                    height = br.ReadUInt16();
                    something = br.ReadByte();
                    Debug.Assert(something == 0);
                    int spriteCount2 = br.ReadByte();
                    Debug.Assert(spriteCount2 == spriteCount);
                }

                var bytes = br.ReadBytes(width * height);
                sprite.Frames[i] = new AlbionSprite.Frame(0, currentY, width, height);
                frameBytes.Add(bytes);

                currentY += height;
                if (width > sprite.Width)
                    sprite.Width = width;
            }

            sprite.Height = currentY;
            sprite.PixelData = new byte[spriteCount * sprite.Width * currentY];

            for (int n = 0; n < spriteCount; n++)
            {
                var frame = sprite.Frames[n];

                for (int j = 0; j < frame.Height; j++)
                for (int i = 0; i < frame.Width; i++)
                    sprite.PixelData[(frame.Y + j) * sprite.Width + frame.X + i] = frameBytes[n][j * frame.Width + i];
            }

            Debug.Assert(br.BaseStream.Position == initialPosition + streamLength);
            return sprite;
        }
    }
}