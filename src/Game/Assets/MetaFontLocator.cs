using System;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Assets
{
    public class MetaFontLocator : IAssetLocator
    {
        readonly ICoreFactory _factory;

        public MetaFontLocator(ICoreFactory factory)
        {
            _factory = factory;
        }

        public IEnumerable<AssetType> SupportedTypes => new[] { AssetType.MetaFont };

        public object LoadAsset(AssetId key, SerializationContext context, Func<AssetId, SerializationContext, object> loaderFunc)
        {
            if (loaderFunc == null) throw new ArgumentNullException(nameof(loaderFunc));
            var regular = (ITexture)loaderFunc((SpriteId)Base.Font.RegularFont, context);
            var bold = (ITexture)loaderFunc((SpriteId)Base.Font.BoldFont, context);
            return FontLoader.Load(_factory, (MetaFontId) key.Id, regular, bold);
        }

        public AssetInfo GetAssetInfo(AssetId key, Func<AssetId, SerializationContext, object> loaderFunc) => null;
    }
}
