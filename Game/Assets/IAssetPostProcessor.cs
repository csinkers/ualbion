using System;
using System.Collections.Generic;
using UAlbion.Core;

namespace UAlbion.Game.Assets
{
    public interface IAssetPostProcessor
    {
        object Process(ICoreFactory factory, AssetKey key, string name, object asset);
        IEnumerable<Type> SupportedTypes { get; }
    }
}
