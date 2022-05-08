using System;
using System.Numerics;
using UAlbion.Core.Veldrid;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Game.Entities;
using UAlbion.Game.Entities.Map2D;
using UAlbion.Game.Veldrid.Visual;

namespace UAlbion.Game.Veldrid;

public class VeldridGameFactory : VeldridCoreFactory, IGameFactory
{
    public IMapLayer CreateMapLayer(LogicalMap2D logicalMap, ITileGraphics tileset, Vector2 tileSize, bool isOverlay)
    {
        if (logicalMap == null) throw new ArgumentNullException(nameof(logicalMap));
        if (tileset == null) throw new ArgumentNullException(nameof(tileset));

        if (tileset is TrueColorTileGraphics trueColor)
            return new BlendedMapLayer(logicalMap, trueColor, tileSize, isOverlay);
        return new SimpleMapLayer(logicalMap, tileset, tileSize, isOverlay);
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