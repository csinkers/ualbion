using SerdesNet;

namespace UAlbion.Config;

public interface IAssetLoader
{
    object Serdes(object existing, ISerdes s, AssetLoadContext context);
}

public interface IAssetLoader<T> : IAssetLoader where T : class
{
    T Serdes(T existing, ISerdes s, AssetLoadContext context); // SerDes = Serialise / Deserialise.
}