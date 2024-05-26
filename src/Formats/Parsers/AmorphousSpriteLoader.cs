using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SerdesNet;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Config.Properties;

namespace UAlbion.Formats.Parsers;

public class AmorphousSpriteLoader : IAssetLoader<IReadOnlyTexture<byte>>
{
    public static readonly StringAssetProperty SubSpritesProperty = new("SubSprites"); 
    static readonly Regex SizesRegex = new(@"
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
            var width = int.Parse(match.Groups["width"].Value);
            var height = int.Parse(match.Groups["height"].Value);
            var countString = match.Groups["count"].Value;

            if (!string.IsNullOrEmpty(countString))
            {
                var count = int.Parse(countString);
                for(int i = 0; i < count; i++)
                    yield return (width, height);
            }
            else
            {
                for(;;) yield return (width, height);
            }
        }
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((IReadOnlyTexture<byte>)existing, s, context);

    public IReadOnlyTexture<byte> Serdes(IReadOnlyTexture<byte> existing, ISerializer s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(s);
        ArgumentNullException.ThrowIfNull(context);
        return s.IsWriting()
            ? Write(existing, s)
            : Read(context, s);
    }

    static IReadOnlyTexture<byte> Write(IReadOnlyTexture<byte> existing, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(existing);

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

    static SimpleTexture<byte> Read(AssetLoadContext context, ISerializer s)
    {
        var sizes = ParseSpriteSizes(context.Node.GetProperty(SubSpritesProperty));

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

        var result = new SimpleTexture<byte>(context.AssetId, totalWidth, totalHeight);

        for (int n = 0; n < frames.Count; n++)
        {
            var (y, w, h) = frames[n];
            result.AddRegion(0, y, w, h);
            BlitUtil.BlitDirect(
                new ReadOnlyImageBuffer<byte>(w, h, w, allFrames[n]),
                result.GetMutableRegionBuffer(n));
        }

        return result;
    }
}
