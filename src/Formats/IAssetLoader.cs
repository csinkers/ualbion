using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats
{
    public interface IAssetLoader
    {
        object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil);
    }

    public interface IAssetLoader<T> : IAssetLoader where T : class
    {
        T Serdes(T existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil); // SerDes = Serialise / Deserialise.
    }
}
