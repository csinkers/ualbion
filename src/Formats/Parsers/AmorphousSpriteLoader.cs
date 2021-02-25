using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class AmorphousSpriteLoader : IAssetLoader<AlbionSprite>
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
            => Serdes((AlbionSprite)existing, config, mapping, s);

        public AlbionSprite Serdes(AlbionSprite existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (s.IsWriting() && existing == null) throw new ArgumentNullException(nameof(existing));
            ApiUtil.Assert(!config.Transposed);

            var sizes = ParseSpriteSizes(config.Get<string>("SubSprites", null));

            int spriteWidth = 0;
            int currentY = 0;
            var allFrames = new List<byte[]>();
            var frames = new List<AlbionSpriteFrame>();

            foreach(var (width, height) in sizes)
            {
                if (s.BytesRemaining <= 0 || (existing != null && existing.Height <= currentY))
                    break;

                byte[] frameBytes = null;
                if (s.IsWriting())
                {
                    frameBytes = new byte[width * height];
                    Debug.Assert(existing != null, nameof(existing) + " != null");

                    FormatUtil.Blit(
                        existing.PixelData.AsSpan(currentY * existing.Width),
                        frameBytes.AsSpan(),
                        width, height,
                        existing.Width, width);
                }

                frameBytes = s.ByteArray("PixelData", frameBytes, width * height);
                frames.Add(new AlbionSpriteFrame(0, currentY, width, height));
                allFrames.Add(frameBytes);

                currentY += height;
                if (width > spriteWidth)
                    spriteWidth = width;
            }

            if (s.IsWriting())
                return existing;

            var spriteHeight = currentY;
            var pixelData = new byte[frames.Count * spriteWidth * currentY];

            for (int n = 0; n < frames.Count; n++)
            {
                var frame = frames[n];
                FormatUtil.Blit(
                    allFrames[n],
                    pixelData.AsSpan(frame.Y*spriteWidth + frame.X),
                    frame.Width, frame.Height,
                    frame.Width, spriteWidth);
            }

            s.Check();
            return new AlbionSprite(config.AssetId.ToString(), spriteWidth, spriteHeight, false, pixelData, frames);
        }
    }
}
