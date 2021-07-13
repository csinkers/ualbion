using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Config;

namespace UAlbion.Game.Assets
{
    public class AlbionSpritePostProcessor : IAssetPostProcessor
    {
        public object Process(object asset, AssetInfo info)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            if (info == null) throw new ArgumentNullException(nameof(info));

            var sprite = (IReadOnlyTexture<byte>)asset;

            // For non-uniforms just use the on-disk packing
            var subImages = new Region[sprite.Regions.Count];
            for (int i = 0; i < subImages.Length; i++)
            {
                var x = sprite.Regions[i];
                subImages[i] = new Region(
                    new Vector2(x.X, x.Y),
                    new Vector2(x.Width, x.Height),
                    new Vector2(sprite.Width, sprite.Height),
                    0);
            }

            return new SimpleTexture<byte>(
                info.AssetId,
                sprite.Id.ToString(),
                sprite.Width, sprite.Height,
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
