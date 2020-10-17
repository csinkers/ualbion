using System;
using UAlbion.Config;
using UAlbion.Core;

namespace UAlbion.Game.Assets
{
    public interface IAssetLocatorRegistry : IComponent
    {
        IAssetLocatorRegistry AddAssetLocator(IAssetLocator locator, bool useAsDefault = false);
        IAssetLocatorRegistry AddAssetPostProcessor(IAssetPostProcessor postProcessor);
        object LoadAsset<T>(T key, SerializationContext context) where T : unmanaged, Enum;
        object LoadAsset(AssetId key, SerializationContext context);
        object LoadAssetCached<T>(T key, SerializationContext context) where T : unmanaged, Enum;
        object LoadAssetCached(AssetId key, SerializationContext context);
        AssetInfo GetAssetInfo(AssetId key);
    }
}
