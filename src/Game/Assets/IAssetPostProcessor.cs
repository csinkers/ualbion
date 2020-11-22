using System;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;

namespace UAlbion.Game.Assets
{
    public interface IAssetPostProcessor
    {
        object Process(ICoreFactory factory, AssetId key, object asset);
        IEnumerable<Type> SupportedTypes { get; }
    }
}
