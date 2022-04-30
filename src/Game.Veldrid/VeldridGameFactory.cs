using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid;
using UAlbion.Game.Entities;
using UAlbion.Game.Entities.Map2D;
using UAlbion.Game.Veldrid.Visual;

namespace UAlbion.Game.Veldrid;

public class VeldridGameFactory : VeldridCoreFactory, IGameFactory
{
    public IMapLayer CreateMapLayer(LogicalMap2D logicalMap, ITexture tileset, bool isOverlay) => new MapLayer(logicalMap, tileset, isOverlay);

    protected override void Subscribed()
    {
        Exchange.Register<IGameFactory>(this);
        base.Subscribed();
    }

    protected override void Unsubscribed()
    {
        base.Unsubscribed();
        Exchange.Unregister(this);
    }
}