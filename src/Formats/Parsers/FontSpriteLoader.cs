using System.Collections.Generic;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class FontSpriteLoader : IAssetLoader
    {
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            var font = (AlbionSprite)new FixedSizeSpriteLoader().Serdes(existing, config, mapping, s);
            var frames = new List<AlbionSpriteFrame>();

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

                frames.Add(new AlbionSpriteFrame(oldFrame.X, oldFrame.Y, width + 2, oldFrame.Height));
            }

            return new AlbionSprite(font.Name, font.Width, font.Height, false, font.PixelData, frames);
        }
    }
}
