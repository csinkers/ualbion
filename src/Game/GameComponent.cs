using UAlbion.Api.Eventing;
using UAlbion.Formats;

namespace UAlbion.Game;

public abstract class GameComponent : Component
{
    protected IAssetManager Assets => Resolve<IAssetManager>();
}

public abstract class GameServiceComponent<T> : ServiceComponent<T>
{
    protected IAssetManager Assets => Resolve<IAssetManager>();
}