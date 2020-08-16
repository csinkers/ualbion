using System.IO;
using SerdesNet;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;

namespace UAlbion.Formats
{
    public interface IAssetLoader
    {
        object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config);
    }

    public interface IAssetLoader<T> : IAssetLoader where T : class
    {
        T Serdes(T existing, ISerializer s, AssetKey key, AssetInfo config); // SerDes = Serialise / Deserialise.
    }
}
