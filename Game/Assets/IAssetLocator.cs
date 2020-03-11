using System;
using System.Collections.Generic;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Assets
{
    public interface IAssetLocator
    {
        object LoadAsset(AssetKey key, string name, Func<AssetKey, string, object> loaderFunc);
        IEnumerable<AssetType> SupportedTypes { get; }
    }
}
