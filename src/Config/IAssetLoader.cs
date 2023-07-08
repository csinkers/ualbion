using SerdesNet;

namespace UAlbion.Config;

public interface IAssetLoader
{
    object Serdes(object existing, ISerializer s, AssetLoadContext context);
}

public interface IAssetLoader<T> : IAssetLoader where T : class
{
    T Serdes(T existing, ISerializer s, AssetLoadContext context); // SerDes = Serialise / Deserialise.
}