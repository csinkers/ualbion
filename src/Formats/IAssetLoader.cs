using System.IO;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats
{
    public interface IAssetLoader
    {
        object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config);
    }

    public interface IAssetLoader<T> : IAssetLoader where T : class
    {
        T Serdes(T existing, AssetMapping mapping, ISerializer s, AssetId id, AssetInfo config); // SerDes = Serialise / Deserialise.
    }
}
