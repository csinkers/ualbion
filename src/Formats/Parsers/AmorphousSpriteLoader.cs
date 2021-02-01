using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
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
            if(s == null)
                yield break;

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

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (s.IsWriting()) throw new NotImplementedException($"Writing of amorphous sprites is not currently supported");
            ApiUtil.Assert(config.Transposed != true);

            var sizes = ParseSpriteSizes(config.Get<string>("SubSprites", null));

            int spriteWidth = 0;
            int currentY = 0;
            var frameBytes = new List<byte[]>();
            var frames = new List<AlbionSpriteFrame>();

            foreach(var (width, height) in sizes)
            {
                if (s.BytesRemaining <= 0)
                    break;

                var bytes = s.ByteArray("PixelData", null, width * height);
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

            s.Check();
            return new AlbionSprite(config.AssetId.ToString(), spriteWidth, spriteHeight, false, pixelData, frames);
        }
    }
}
