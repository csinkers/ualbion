using System;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;

namespace UAlbion.Game.Assets
{
    public class IntToStringLocator : Component, IAssetLocator
    {
        public object LoadAsset(AssetId key, SerializationContext context, AssetInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            var modApplier = Resolve<IModApplier>();
            var sourceAssetName = info.File.EnumType.LocatorArg;
            var sourceId = AssetId.Parse(sourceAssetName); // AssetId.From(Base.Special.SystemStrings)
            var source = (IDictionary<int, string>)modApplier.LoadAssetCached(sourceId);
            return source?[key.Id];
        }
    }
}