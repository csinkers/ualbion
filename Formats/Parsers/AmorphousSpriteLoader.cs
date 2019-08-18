using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(XldObjectType.AmorphousSprite)]
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

        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            Debug.Assert(config.Parent.RotatedLeft != true);
            long initialPosition = br.BaseStream.Position;
            var sizes = ParseSpriteSizes(config.SubSprites);

            AlbionSprite sprite = new AlbionSprite
            {
                Name = name,
                Width = 0,
                UniformFrames = false,
                Frames = new List<AlbionSprite.Frame>()
            };

            int currentY = 0;
            var frameBytes = new List<byte[]>();
            foreach(var (width, height) in sizes)
            {
                if (br.BaseStream.Position >= initialPosition + streamLength)
                    break;

                var bytes = br.ReadBytes(width * height);
                sprite.Frames.Add(new AlbionSprite.Frame(0, currentY, width, height));
                frameBytes.Add(bytes);

                currentY += height;
                if (width > sprite.Width)
                    sprite.Width = width;
            }

            sprite.Height = currentY;
            sprite.PixelData = new byte[sprite.Frames.Count * sprite.Width * currentY];

            for (int n = 0; n < sprite.Frames.Count; n++)
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