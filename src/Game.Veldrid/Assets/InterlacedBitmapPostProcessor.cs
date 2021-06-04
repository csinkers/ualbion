using System;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Visual;
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
            var texture =
                new Texture<uint>(info.AssetId, bitmap.Width, bitmap.Height)
                .AddRegion(0, 0, bitmap.Width, bitmap.Height);

            var imageBuffer = new ReadOnlyImageBuffer<byte>(bitmap.Width, bitmap.Height, bitmap.Width, bitmap.ImageData);
            BlitUtil.BlitTiled8To32(imageBuffer, texture.GetMutableLayerBuffer(0), bitmap.Palette, 255, null);
            return texture;
        }
    }
}
