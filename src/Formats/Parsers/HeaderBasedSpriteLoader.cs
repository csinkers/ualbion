using System;
using System.Collections.Generic;
using System.IO;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.SingleHeaderSprite, FileFormat.HeaderPerSubImageSprite)]
    public class HeaderBasedSpriteLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
        {
            if (br == null) throw new ArgumentNullException(nameof(br));
            if (config == null) throw new ArgumentNullException(nameof(config));

            ApiUtil.Assert(config.Transposed != true);
            long initialPosition = br.BaseStream.Position;

            int width = br.ReadUInt16();
            int height = br.ReadUInt16();
            int something = br.ReadByte();
            ApiUtil.Assert(something == 0);
            int spriteCount = br.ReadByte();

            bool uniform = config.Format == FileFormat.SingleHeaderSprite;
            var frames = new AlbionSpriteFrame[spriteCount];
            var frameBytes = new List<byte[]>();
            int currentY = 0;

            int spriteWidth = 0;
            for (int i = 0; i < spriteCount; i++)
            {
                if (!uniform && i > 0)
                {
                    width = br.ReadUInt16();
                    height = br.ReadUInt16();
                    something = br.ReadByte();
                    ApiUtil.Assert(something == 0);
                    int spriteCount2 = br.ReadByte();
                    ApiUtil.Assert(spriteCount2 == spriteCount);
                }

                var bytes = br.ReadBytes(width * height);
                frames[i] = new AlbionSpriteFrame(0, currentY, width, height);
                frameBytes.Add(bytes);

                currentY += height;
                if (width > spriteWidth)
                    spriteWidth = width;
            }

            byte[] pixelData = new byte[spriteCount * spriteWidth * currentY];
            for (int n = 0; n < spriteCount; n++)
            {
                var frame = frames[n];

                for (int j = 0; j < frame.Height; j++)
                for (int i = 0; i < frame.Width; i++)
                    pixelData[(frame.Y + j) * spriteWidth + frame.X + i] = frameBytes[n][j * frame.Width + i];
            }

            ApiUtil.Assert(br.BaseStream.Position == initialPosition + streamLength);
            return new AlbionSprite(key.ToString(), spriteWidth, currentY, uniform, pixelData, frames);
        }
    }
}
