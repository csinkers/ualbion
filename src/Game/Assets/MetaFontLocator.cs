using System;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Assets
{
    public class MetaFontLoader : Component
    {
        readonly ICoreFactory _factory;
        public MetaFontLoader(ICoreFactory factory) => _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        public object Load(AssetId id)
        {
            var assets = Resolve<IAssetManager>();
            var regular = assets.LoadTexture(Base.Font.RegularFont);
            var bold = assets.LoadTexture(Base.Font.BoldFont);
            return FontLoader.Load(_factory, (MetaFontId)id.Id, regular, bold);
        }
    }
}
