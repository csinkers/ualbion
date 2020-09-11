using System;
using System.Collections.Generic;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    public interface IAssetLocator
    {
        object LoadAsset(AssetKey key, string name, Func<AssetKey, object> loaderFunc);
        AssetInfo GetAssetInfo(AssetKey key, Func<AssetKey, object> loaderFunc);
        IEnumerable<AssetType> SupportedTypes { get; }
    }
}
