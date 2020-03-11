using System.Collections.Generic;
using System.IO;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.Font)]
    public class FontSpriteLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
        {
            var font = (AlbionSprite)new FixedSizeSpriteLoader().Load(br, streamLength, name, config);
            var frames = new List<AlbionSprite.Frame>();

            // Fix up sub-images for variable size
            foreach (var oldFrame in font.Frames)
            {
                int width = 0;
                for (int j = oldFrame.Y; j < oldFrame.Y + oldFrame.Height; j++)
                {
                    for (int i = oldFrame.X; i < oldFrame.X + oldFrame.Width; i++)
                    {
                        if (i - oldFrame.X > width && font.PixelData[j * font.Width + i] != 0)
                            width = i - oldFrame.X;
                    }
                }

                frames.Add(new AlbionSprite.Frame(oldFrame.X, oldFrame.Y, width + 2, oldFrame.Height));
            }

            font.UniformFrames = false;
            font.Frames = frames;
            return font;
        }
    }
}
