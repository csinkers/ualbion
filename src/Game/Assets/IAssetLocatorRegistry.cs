using UAlbion.Config;
using UAlbion.Core;

namespace UAlbion.Game.Assets
{
    public interface IAssetLocatorRegistry : IComponent
    {
        IAssetLocatorRegistry AddAssetLocator(IAssetLocator locator, bool useAsDefault = false);
        IAssetLocatorRegistry AddAssetPostProcessor(IAssetPostProcessor postProcessor);
        object LoadAsset(AssetId key, SerializationContext context, AssetInfo info);
    }
}
