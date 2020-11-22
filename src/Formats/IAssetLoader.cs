using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats
{
    public interface IAssetLoader
    {
        object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s);
    }

    public interface IAssetLoader<T> : IAssetLoader where T : class
    {
        T Serdes(T existing, AssetInfo config, AssetMapping mapping, ISerializer s); // SerDes = Serialise / Deserialise.
    }
}
