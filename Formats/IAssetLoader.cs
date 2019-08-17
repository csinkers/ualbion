using System.IO;

namespace UAlbion.Formats
{
    public interface IAssetLoader
    {
        object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config);
    }
}