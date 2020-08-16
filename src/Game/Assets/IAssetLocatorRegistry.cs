using UAlbion.Core;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Assets
{
    public interface IAssetLocatorRegistry : IComponent
    {
        IAssetLocatorRegistry AddAssetLocator(IAssetLocator locator, bool useAsDefault = false);
        IAssetLocatorRegistry AddAssetPostProcessor(IAssetPostProcessor postProcessor);
        object LoadAsset(AssetKey key);
        object LoadAssetCached(AssetKey key);
    }
}