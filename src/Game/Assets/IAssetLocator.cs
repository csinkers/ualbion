using System;
using System.Collections.Generic;
using UAlbion.Config;

namespace UAlbion.Game.Assets
{
    public interface IAssetLocator
    {
        object LoadAsset(AssetId key, SerializationContext context, Func<AssetId, SerializationContext, object> loaderFunc);
        AssetInfo GetAssetInfo(AssetId key, Func<AssetId, SerializationContext, object> loaderFunc);
        IEnumerable<AssetType> SupportedTypes { get; }
    }
}
