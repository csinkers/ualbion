using System;
using SerdesNet;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class SlabLoader : IAssetLoader<IEightBitImage>
    {
        const int StatusBarHeight = 48;
        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((IEightBitImage)existing, info, mapping, s);

        public IEightBitImage Serdes(IEightBitImage existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            IEightBitImage singleFrame = null;
            if (s.IsWriting())
            {
                if (existing == null) throw new ArgumentNullException(nameof(existing));
                singleFrame = new AlbionSprite(
                    AssetId.FromUInt32(existing.Id.ToUInt32()),
                    existing.Width, existing.Height, true,
                    existing.PixelData,
                    new[] { new AlbionSpriteFrame(
                        existing.GetSubImage(0).X,
                        existing.GetSubImage(0).Y,
                        existing.GetSubImage(0).Width,
                        existing.GetSubImage(0).Height,
                        existing.Width)
                    }
                );
            }

            var sprite = new FixedSizeSpriteLoader().Serdes(singleFrame, info, mapping, s);
            if (sprite == null)
                return null;

            var frames = new[] // Frame 0 = entire slab, Frame 1 = status bar only.
            {
                new AlbionSpriteFrame(0, 0, sprite.Width, sprite.Height, sprite.Width),
                new AlbionSpriteFrame(0, sprite.Height - StatusBarHeight, sprite.Width, StatusBarHeight, sprite.Width)
            };

            return new AlbionSprite(
                AssetId.FromUInt32(sprite.Id.ToUInt32()),
                sprite.Width,
                sprite.Height,
                false,
                sprite.PixelData,
                frames);
        }
    }
}
