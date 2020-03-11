using System.IO;
using SerdesNet;
using UAlbion.Formats.Config;

namespace UAlbion.Formats
{
    public interface IAssetLoader
    {
        object Load(BinaryReader br, long streamLength, string name, AssetInfo config);
    }

    public interface IAssetLoader<T> : IAssetLoader
    {
        T Serdes(T existing, ISerializer s, string name, AssetInfo config); // SerDes = Serialise / Deserialise.
    }
}
