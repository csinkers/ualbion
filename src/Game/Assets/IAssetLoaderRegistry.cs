using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats;

namespace UAlbion.Game.Assets
{
    public interface IAssetLoaderRegistry
    {
        IAssetLoader GetLoader(FileFormat type);
        IAssetLoader<T> GetLoader<T>(FileFormat type) where T : class;
        object Load(AssetInfo config, AssetMapping mapping, ISerializer s);
    }
}
