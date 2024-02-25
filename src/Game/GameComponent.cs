using UAlbion.Api.Eventing;
using UAlbion.Formats;

namespace UAlbion.Game;

public abstract class GameComponent : Component
{
    protected static ushort Random() => AlbionRandom.Next();
    protected IAssetManager Assets => Resolve<IAssetManager>();
}

public abstract class GameServiceComponent<T> : ServiceComponent<T>
{
    protected IAssetManager Assets => Resolve<IAssetManager>();
}