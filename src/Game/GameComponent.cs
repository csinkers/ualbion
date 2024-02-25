using UAlbion.Api.Eventing;
using UAlbion.Formats;

namespace UAlbion.Game;

public abstract class GameComponent : Component
{
    protected IAssetManager Assets => Resolve<IAssetManager>();
}