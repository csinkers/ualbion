using System;
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Entities;
using UAlbion.Game.Entities.Map2D;
using UAlbion.Game.Veldrid.Visual;

namespace UAlbion.Game.Veldrid;

public class VeldridGameFactory : VeldridCoreFactory, IGameFactory
{
    public IMapLayer CreateMapLayer(
        LogicalMap2D logicalMap,
        ITexture tileset,
        Func<int, TileData> getTileFunc,
        DrawLayer layer,
        IconChangeType iconChangeType)
    {
        return new MapLayer(logicalMap, tileset, getTileFunc, layer, iconChangeType);
    }

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