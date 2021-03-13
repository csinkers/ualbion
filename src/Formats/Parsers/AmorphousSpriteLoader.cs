using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using SerdesNet;
using UAlbion.Api.Visual;
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

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((IEightBitImage)existing, info, mapping, s);

        public IEightBitImage Serdes(IEightBitImage existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (info == null) throw new ArgumentNullException(nameof(info));
            return s.IsWriting()
                ? Write(existing, s)
                : Read(info, s);
        }

        static IEightBitImage Write(IEightBitImage existing, ISerializer s)
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));

            int bufferW = 0, bufferH = 0;
            for (int i = 0; i < existing.SubImageCount; i++)
            {
                var frame = existing.GetSubImage(i);
                if (frame.Width > bufferW) bufferW = frame.Width;
                if (frame.Height > bufferH) bufferH = frame.Height;
            }

            var buffer = new byte[bufferW * bufferH];
            for(int i = 0; i < existing.SubImageCount; i++)
            {
                var frame = existing.GetSubImage(i);
                FormatUtil.Blit(
                    existing.PixelData.AsSpan(frame.PixelOffset, frame.PixelLength),
                    buffer.AsSpan(),
                    frame.Width, frame.Height,
                    existing.Width, frame.Width);

                s.Bytes("PixelData", buffer, frame.Width * frame.Height);
            }

            return existing;
        }

        static IEightBitImage Read(AssetInfo info, ISerializer s)
        {
            var sizes = ParseSpriteSizes(info.Get<string>(AssetProperty.SubSprites, null));

            int totalWidth = 0;
            int currentY = 0;
            var allFrames = new List<byte[]>();
            var frames = new List<(int, int, int)>(); // (y, w, h)

            foreach(var (width, height) in sizes)
            {
                if (s.BytesRemaining <= 0)
                    break;

                byte[] frameBytes = s.Bytes("PixelData", null, width * height);
                frames.Add((currentY, width, height));
                allFrames.Add(frameBytes);

                currentY += height;
                if (width > totalWidth)
                    totalWidth = width;
            }

            var totalHeight = currentY;
            var pixelData = new byte[totalWidth * totalHeight];

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
            return new AlbionSprite(
                info.AssetId,
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
