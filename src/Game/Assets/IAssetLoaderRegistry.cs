using System.IO;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    public interface IAssetLoaderRegistry
    {
        IAssetLoader GetLoader(FileFormat type);
        IAssetLoader<T> GetLoader<T>(FileFormat type) where T : class;
        object Load(BinaryReader br, AssetKey key, int streamLength, AssetInfo config);
    }
}