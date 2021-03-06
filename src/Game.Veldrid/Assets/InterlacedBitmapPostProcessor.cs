using System;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats.Assets;
using UAlbion.Game.Assets;

namespace UAlbion.Game.Veldrid.Assets
{
    public class InterlacedBitmapPostProcessor : IAssetPostProcessor
    {
        public object Process(object asset, AssetInfo info, ICoreFactory factory)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            if (info == null) throw new ArgumentNullException(nameof(info));

            var bitmap = (InterlacedBitmap)asset;
            return new TrueColorTexture(
                info.AssetId, info.AssetId.ToString(),
                bitmap.Width, bitmap.Height,
                bitmap.Palette, bitmap.ImageData);
        }
    }
}
