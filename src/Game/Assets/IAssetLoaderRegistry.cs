using System.IO;
using UAlbion.Config;
using UAlbion.Formats;

namespace UAlbion.Game.Assets
{
    public interface IAssetLoaderRegistry
    {
        IAssetLoader GetLoader(FileFormat type);
        IAssetLoader<T> GetLoader<T>(FileFormat type) where T : class;
        object Load(BinaryReader br, AssetId key, int streamLength, AssetMapping mapping, AssetInfo config);
    }
}
