using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Assets
{
    public class MetaFontLocator : Component, IAssetLocator
    {
        readonly ICoreFactory _factory;

        public MetaFontLocator(ICoreFactory factory)
        {
            _factory = factory;
        }

        public IEnumerable<AssetType> SupportedTypes => new[] { AssetType.MetaFont };

        public object LoadAsset(AssetId key, SerializationContext context, AssetInfo info)
        {
            var assets = Resolve<IAssetManager>();
            var regular = assets.LoadTexture(Base.Font.RegularFont);
            var bold = assets.LoadTexture(Base.Font.BoldFont); // (ITexture)loaderFunc((SpriteId)Base.Font.BoldFont, context);
            return FontLoader.Load(_factory, (MetaFontId) key.Id, regular, bold);
        }
    }
}
