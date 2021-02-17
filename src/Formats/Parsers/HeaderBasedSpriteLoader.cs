using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class HeaderBasedSpriteLoader : IAssetLoader<AlbionSprite>
    {
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((AlbionSprite)existing, config, mapping, s);

        public AlbionSprite Serdes(AlbionSprite existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (config == null) throw new ArgumentNullException(nameof(config));

            ApiUtil.Assert(config.Transposed != true);

            int width = s.UInt16(null, 0);
            int height = s.UInt16(null, 0);
            int something = s.UInt8(null, 0);
            ApiUtil.Assert(something == 0);
            int spriteCount = s.UInt8(null, 0);

            bool uniform = config.File?.Format != "NonUniform";
            var frames = new AlbionSpriteFrame[spriteCount];
            var frameBytes = new List<byte[]>();
            int currentY = 0;

            int spriteWidth = 0;
            for (int i = 0; i < spriteCount; i++)
            {
                if (!uniform && i > 0)
                {
                    width = s.UInt16(null, 0);
                    height = s.UInt16(null, 0);
                    something = s.UInt8(null, 0);
                    ApiUtil.Assert(something == 0);
                    int spriteCount2 = s.UInt8(null, 0);
                    ApiUtil.Assert(spriteCount2 == spriteCount);
                }

                var bytes = s.ByteArray(null, null, width * height);
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

            s.Check();
            return new AlbionSprite(config.AssetId.ToString(), spriteWidth, currentY, uniform, pixelData, frames);
        }
    }
}
