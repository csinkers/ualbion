using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats;

public interface IAssetLoader
{
    object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context);
}

public interface IAssetLoader<T> : IAssetLoader where T : class
{
    T Serdes(T existing, AssetInfo info, ISerializer s, SerdesContext context); // SerDes = Serialise / Deserialise.
}