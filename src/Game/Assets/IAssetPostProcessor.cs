using System;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;

namespace UAlbion.Game.Assets
{
    public interface IAssetPostProcessor
    {
        object Process(ICoreFactory factory, AssetId key, object asset, SerializationContext context, Func<AssetId, SerializationContext, object> loaderFunc);
        IEnumerable<Type> SupportedTypes { get; }
    }
}
