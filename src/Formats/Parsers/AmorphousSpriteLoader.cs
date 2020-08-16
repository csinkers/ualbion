using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.AmorphousSprite)]
    public class AmorphousSpriteLoader : IAssetLoader
    {
        static readonly Regex SizesRegex = new Regex(@"
            \(\s*
                (?'width'\d+),\s*
                (?'height'\d+)\s*
                (,\s*(?'count'\d+))?\s*
            \)", RegexOptions.IgnorePatternWhitespace);
        static IEnumerable<(int, int)> ParseSpriteSizes(string s)
        {
            var matches = SizesRegex.Matches(s);
            foreach (Match match in matches)
            {
                var width = int.Parse(match.Groups["width"].Value, CultureInfo.InvariantCulture);
                var height = int.Parse(match.Groups["height"].Value, CultureInfo.InvariantCulture);
                var countString = match.Groups["count"].Value;

                if (!string.IsNullOrEmpty(countString))
                {
                    var count = int.Parse(countString, CultureInfo.InvariantCulture);
                    for(int i = 0; i < count; i++)
                        yield return (width, height);
                }
                else
                {
                    for(;;) yield return (width, height);
                }
            }
        }

        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
        {
            if (br == null) throw new ArgumentNullException(nameof(br));
            if (config == null) throw new ArgumentNullException(nameof(config));
            ApiUtil.Assert(config.Transposed != true);
            long initialPosition = br.BaseStream.Position;
            var sizes = ParseSpriteSizes(config.SubSprites);

            int spriteWidth = 0;
            int currentY = 0;
            var frameBytes = new List<byte[]>();
            var frames = new List<AlbionSpriteFrame>();

            foreach(var (width, height) in sizes)
            {
                if (br.BaseStream.Position >= initialPosition + streamLength)
                    break;

                var bytes = br.ReadBytes(width * height);
                frames.Add(new AlbionSpriteFrame(0, currentY, width, height));
                frameBytes.Add(bytes);

                currentY += height;
                if (width > spriteWidth)
                    spriteWidth = width;
            }

            var spriteHeight = currentY;
            var pixelData = new byte[frames.Count * spriteWidth * currentY];

            for (int n = 0; n < frames.Count; n++)
            {
                var frame = frames[n];

                for (int j = 0; j < frame.Height; j++)
                    for (int i = 0; i < frame.Width; i++)
                        pixelData[(frame.Y + j) * spriteWidth + frame.X + i] = frameBytes[n][j * frame.Width + i];
            }

            ApiUtil.Assert(br.BaseStream.Position == initialPosition + streamLength);
            return new AlbionSprite(key.ToString(), spriteWidth, spriteHeight, false, pixelData, frames);
        }
    }
}
