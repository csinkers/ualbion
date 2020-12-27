using System;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats.Assets;
using UAlbion.Game.Assets;

namespace UAlbion.Game.Veldrid.Assets
{
    public class InterlacedBitmapPostProcessor : IAssetPostProcessor
    {
        public IEnumerable<Type> SupportedTypes => new[] { typeof(InterlacedBitmap) };
        public object Process(ICoreFactory factory, AssetId key, object asset)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            var bitmap = (InterlacedBitmap)asset;
            return new TrueColorTexture(key,
                key.ToString(), (uint)bitmap.Width, (uint)bitmap.Height,
                bitmap.Palette, bitmap.ImageData);
        }
    }
}
