using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Assets
{
    public interface IAssetPostProcessor
    {
        object Process(ICoreFactory factory, AssetKey key, object asset, Func<AssetKey, object> loaderFunc);
        IEnumerable<Type> SupportedTypes { get; }
    }
}
