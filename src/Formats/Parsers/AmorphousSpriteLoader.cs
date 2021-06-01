using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using SerdesNet;
using UAlbion.Api.Visual;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers
{
    public class AmorphousSpriteLoader : IAssetLoader<IReadOnlyTexture<byte>>
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
            => Serdes((IReadOnlyTexture<byte>)existing, info, mapping, s);

        public IReadOnlyTexture<byte> Serdes(IReadOnlyTexture<byte> existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (info == null) throw new ArgumentNullException(nameof(info));
            return s.IsWriting()
                ? Write(existing, s)
                : Read(info, s);
        }

        static IReadOnlyTexture<byte> Write(IReadOnlyTexture<byte> existing, ISerializer s)
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));

            int bufferW = 0, bufferH = 0;
            for (int i = 0; i < existing.Regions.Count; i++)
            {
                var frame = existing.Regions[i];
                if (frame.Width > bufferW) bufferW = frame.Width;
                if (frame.Height > bufferH) bufferH = frame.Height;
            }

            var buffer = new byte[bufferW * bufferH];
            for(int i = 0; i < existing.Regions.Count; i++)
            {
                var frame = existing.GetRegionBuffer(i);
                BlitUtil.BlitDirect(
                    frame,
                    new ImageBuffer<byte>(frame.Width, frame.Height, frame.Width, buffer));

                s.Bytes("PixelData", buffer, frame.Width * frame.Height);
            }

            return existing;
        }

        static IReadOnlyTexture<byte> Read(AssetInfo info, ISerializer s)
        {
            var sizes = ParseSpriteSizes(info.Get<string>(AssetProperty.SubSprites, null));

            int totalWidth = 0;
            int totalHeight = 0;
            var allFrames = new List<byte[]>();
            var frames = new List<(int y, int w, int h)>();

            foreach (var (width, height) in sizes)
            {
                if (s.BytesRemaining <= 0)
                    break;

                byte[] frameBytes = s.Bytes("PixelData", null, width * height);
                frames.Add((totalHeight, width, height));
                allFrames.Add(frameBytes);

                totalHeight += height;
                if (width > totalWidth)
                    totalWidth = width;
            }

            var result = new Texture<byte>(info.AssetId, totalWidth, totalHeight);

            for (int n = 0; n < frames.Count; n++)
            {
                var (y, w, h) = frames[n];
                result.AddRegion(0, y, w, h);
                BlitUtil.BlitDirect(
                    new ReadOnlyImageBuffer<byte>(w, h, w, allFrames[n]),
                    result.GetMutableRegionBuffer(n));
            }

            s.Check();
            return result;
        }
    }
}
