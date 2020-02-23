using System.IO;
using UAlbion.Formats.Config;
using UAlbion.Formats.Parsers;

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