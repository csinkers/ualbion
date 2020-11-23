using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats;

namespace UAlbion.Game.Assets
{
    public interface IAssetLoaderRegistry
    {
        IAssetLoader GetLoader(string loaderName);
        IAssetLoader<T> GetLoader<T>(string loaderName) where T : class;
        object Load(AssetInfo config, AssetMapping mapping, ISerializer s);
    }
}
