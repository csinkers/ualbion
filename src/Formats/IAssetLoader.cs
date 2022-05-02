using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats;

public interface IAssetLoader
{
    object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context);
}

public interface IAssetLoader<T> : IAssetLoader where T : class
{
    T Serdes(T existing, AssetInfo info, ISerializer s, LoaderContext context); // SerDes = Serialise / Deserialise.
}