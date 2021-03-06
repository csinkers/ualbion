using System;
using System.Linq;
using System.Numerics;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Assets
{
    public class AlbionSpritePostProcessor : IAssetPostProcessor
    {
        public object Process(object asset, AssetInfo info, ICoreFactory factory)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            var sprite = (AlbionSprite2)asset;

            // For non-uniforms just use the on-disk packing
            var subImages = sprite.Frames
                .Select(x => new SubImage(
                    new Vector2(x.X, x.Y),
                    new Vector2(x.Width, x.Height),
                    new Vector2(sprite.Width, sprite.Height),
                    0))
                .ToArray();

            return factory.CreateEightBitTexture(
                info.AssetId,
                sprite.Id.ToString(),
                sprite.Width,
                sprite.Height,
                1,
                1,
                sprite.PixelData,
                subImages);
        }

        /*
        else if (sprite.UniformFrames) // For reasonably sized uniform sprites use layers to simplify mip mapping / tiling etc
        {
            int tileWidth = sprite.Width;
            int tileHeight = sprite.Height / sprite.Frames.Count;
            subImages = sprite.Frames
                .Select((x, i) => new EightBitTexture.SubImage(0, 0, x.Width, x.Height, i))
                .ToArray();

            return new EightBitTexture(
                sprite.Name,
                (uint)tileWidth,
                (uint)tileHeight,
                1,
                (uint)subImages.Length,
                 sprite.PixelData, subImages);
        }*/
    }
}
