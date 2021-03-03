using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class SlabLoader : IAssetLoader<AlbionSprite>
    {
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((AlbionSprite)existing, config, mapping, s);

        public AlbionSprite Serdes(AlbionSprite existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            AlbionSprite singleFrame = null;
            if (s.IsWriting())
            {
                if (existing == null) throw new ArgumentNullException(nameof(existing));
                singleFrame = new AlbionSprite(
                    existing.Id, existing.Width, existing.Height, true, existing.PixelData,
                    new[] { new AlbionSpriteFrame(
                        existing.Frames[0].X,
                        existing.Frames[0].Y,
                        existing.Frames[0].Width,
                        existing.Frames[0].Height)
                    }
                );
            }

            var sprite = new FixedSizeSpriteLoader().Serdes(singleFrame, config, mapping, s);
            if (sprite == null)
                return null;

            var frames = new[] // Frame 0 = entire slab, Frame 1 = status bar only.
            {
                sprite.Frames[0],
                new AlbionSpriteFrame(0, sprite.Height - 48, sprite.Width, 48)
            };

            return new AlbionSprite(sprite.Id, sprite.Width, sprite.Height, sprite.UniformFrames, sprite.PixelData, frames);
        }
    }
}
