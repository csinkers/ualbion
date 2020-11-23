using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;

namespace UAlbion.Game.Assets
{
    public class SystemTextLocator : Component, IAssetLocator
    {
        public object LoadAsset(AssetId key, SerializationContext context, AssetInfo info)
        {
            var modApplier = Resolve<IModApplier>();
            var all = (IDictionary<int, string>)modApplier.LoadAssetCached(AssetId.From(Base.Special.SystemStrings));
            return all?[key.Id];
        }
    }
}