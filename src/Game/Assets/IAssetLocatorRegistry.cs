using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    public interface IAssetLocatorRegistry : IComponent
    {
        IAssetLocatorRegistry AddAssetLocator(IAssetLocator locator, bool useAsDefault = false);
        IAssetLocatorRegistry AddAssetPostProcessor(IAssetPostProcessor postProcessor);
        object LoadAsset(AssetKey key);
        object LoadAssetCached(AssetKey key);
        AssetInfo GetAssetInfo(AssetKey key);
    }
}