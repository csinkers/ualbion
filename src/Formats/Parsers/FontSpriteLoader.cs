using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class FontSpriteLoader : IAssetLoader<AlbionSprite>
    {
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((AlbionSprite)existing, config, mapping, s);

        public AlbionSprite Serdes(AlbionSprite existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            AlbionSprite uniformFrames = null;
            if (s.IsWriting())
            {
                if (existing == null) throw new ArgumentNullException(nameof(existing));
                uniformFrames = new AlbionSprite(
                    existing.Name,
                    existing.Width,
                    existing.Height,
                    true,
                    existing.PixelData,
                    existing.Frames.Select(x => new AlbionSpriteFrame(x.X, x.Y, config.Width, config.Height)).ToList());
            }

            var font = new FixedSizeSpriteLoader().Serdes(uniformFrames, config, mapping, s);
            if (font == null)
                return null;

            if (s.IsWriting())
                return existing;

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
