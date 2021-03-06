using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class AmorphousSpriteLoader : IAssetLoader<IEightBitImage>
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
            => Serdes((IEightBitImage)existing, config, mapping, s);

        public IEightBitImage Serdes(IEightBitImage existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var sizes = ParseSpriteSizes(config.Get<string>(AssetProperty.SubSprites, null));

            int totalWidth = 0;
            int currentY = 0;
            var allFrames = new List<byte[]>();
            var frames = new List<(int, int, int)>(); // (y, w, h)

            foreach(var (width, height) in sizes)
            {
                if (s.BytesRemaining <= 0 || (existing != null && existing.Height <= currentY))
                    break;

                byte[] frameBytes = null;
                if (s.IsWriting())
                {
                    if (existing == null) throw new ArgumentNullException(nameof(existing));
                    frameBytes = new byte[width * height];

                    FormatUtil.Blit(
                        existing.PixelData.Slice(currentY * existing.Width),
                        frameBytes.AsSpan(),
                        width, height,
                        existing.Width, width);
                }

                frameBytes = s.ByteArray("PixelData", frameBytes, width * height);
                frames.Add((currentY, width, height));
                allFrames.Add(frameBytes);

                currentY += height;
                if (width > totalWidth)
                    totalWidth = width;
            }

            if (s.IsWriting())
                return existing;

            var totalHeight = currentY;
            var pixelData = new byte[frames.Count * totalWidth * currentY];

            for (int n = 0; n < frames.Count; n++)
            {
                var (y, w, h) = frames[n];
                FormatUtil.Blit(
                    allFrames[n],
                    pixelData.AsSpan(y * totalWidth),
                    w, h,
                    w, totalWidth);
            }

            s.Check();
            return new AlbionSprite2(
                config.AssetId,
                totalWidth,
                totalHeight,
                false,
                pixelData,
                frames.Select(frame =>
                {
                    var (y,w,h) = frame;
                    return new AlbionSpriteFrame(0, y, w, h, totalWidth);
                }));
        }
    }
}
