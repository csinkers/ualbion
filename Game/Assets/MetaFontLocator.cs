using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Assets
{
    public class MetaFontLocator : IAssetLocator
    {
        readonly ICoreFactory _factory;

        public MetaFontLocator(ICoreFactory factory)
        {
            _factory = factory;
        }

        public IEnumerable<AssetType> SupportedTypes => new[] {AssetType.MetaFont};

        public object LoadAsset(AssetKey key, string name, Func<AssetKey, string, object> loaderFunc)
        {
            var regular = (ITexture)loaderFunc(new AssetKey(AssetType.Font), "RegularFont");
            var bold = (ITexture)loaderFunc(new AssetKey(AssetType.Font, (int)FontId.BoldFont), "BoldFont");
            return FontLoader.Load(_factory, (MetaFontId) key.Id, regular, bold);
        }
    }
}
