using System.IO;
using UAlbion.Formats.Config;

namespace UAlbion.Formats
{
    public interface IAssetLoader
    {
        object Load(BinaryReader br, long streamLength, string name, AssetInfo config);
        // object Serdes(object existing, ISerializer s, string name, AssetInfo config); // SerDes = Serialise / Deserialise.
    }
}