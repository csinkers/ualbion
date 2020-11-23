using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Assets
{
    public class UAlbionStringLocator : Component, IAssetLocator
    {
        public object LoadAsset(AssetId key, SerializationContext context, AssetInfo info)
        {
            var modApplier = Resolve<IModApplier>();
            var all = (IDictionary<TextId, string>)modApplier.LoadAssetCached(AssetId.From(Base.Special.UAlbionStrings));
            return all[key];
        }
    }
}