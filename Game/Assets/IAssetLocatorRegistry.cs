using System;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Assets
{
    public interface IAssetLocatorRegistry : IComponent, IDisposable
    {
        IAssetLocatorRegistry AddAssetLocator(IAssetLocator locator);
        IAssetLocatorRegistry AddAssetPostProcessor(IAssetPostProcessor postProcessor);
        object LoadAsset(AssetKey key);
        object LoadAssetCached(AssetKey key);
    }
}