using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;

namespace UAlbion.Game.Assets
{
    public class AlbionSpritePostProcessor : IAssetPostProcessor
    {
        public object Process(object asset, AssetInfo info, ICoreFactory factory)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            var sprite = (IEightBitImage)asset;

            // For non-uniforms just use the on-disk packing
            var subImages = new SubImage[sprite.SubImageCount];
            for (int i = 0; i < subImages.Length; i++)
            {
                var x = sprite.GetSubImage(i);
                subImages[i] = new SubImage(
                    new Vector2(x.X, x.Y),
                    new Vector2(x.Width, x.Height),
                    new Vector2(sprite.Width, sprite.Height),
                    0);
            }

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
