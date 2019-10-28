using System;

namespace UAlbion.Game.Assets
{
    public interface IAssetLocator
    {
        object LoadAsset(AssetKey key, string name, Func<AssetKey, string, object> loaderFunc);
    }
}