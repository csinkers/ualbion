using System;
using UAlbion.Core.Textures;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Assets
{
    [AssetLocator(AssetType.MetaFont)]
    public class MetaFontLocator : IAssetLocator
    {
        public object LoadAsset(AssetKey key, string name, Func<AssetKey, string, object> loaderFunc)
        {
            var regular = (ITexture)loaderFunc(new AssetKey(AssetType.Font, (int)FontId.RegularFont), "RegularFont");
            var bold = (ITexture)loaderFunc(new AssetKey(AssetType.Font, (int)FontId.BoldFont), "BoldFont");
            return FontLoader.Load((MetaFontId) key.Id, regular, bold);
        }
    }
}