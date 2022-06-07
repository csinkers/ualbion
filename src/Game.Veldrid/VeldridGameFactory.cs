using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Game.Entities;
using UAlbion.Game.Entities.Map2D;
using UAlbion.Game.Veldrid.Visual;

namespace UAlbion.Game.Veldrid;

public class VeldridGameFactory : VeldridCoreFactory, IGameFactory
{
    //*
    public IMapLayer CreateMapLayer(LogicalMap2D logicalMap, ITileGraphics tileset, Vector2 tileSize)
    {
        if (logicalMap == null) throw new ArgumentNullException(nameof(logicalMap));
        if (tileset == null) throw new ArgumentNullException(nameof(tileset));
        return new TileRendererMapLayer(logicalMap, tileset, tileSize, DrawLayer.Underlay);
    }

    /*/
    public IMapLayer CreateMapLayer(
        MapLayerType type,
        LogicalMap2D logicalMap,
        ITileGraphics tileset,
        Vector2 tileSize,
        DrawLayer renderOrder)
    {
        if (logicalMap == null) throw new ArgumentNullException(nameof(logicalMap));
        if (tileset == null) throw new ArgumentNullException(nameof(tileset));

        if (tileset is TrueColorTileGraphics trueColor)
        {
            var builder = new BlendedMapLayerInfoBuilder(trueColor, tileSize);
            var behavior = CreateMapLayerBehavior(type, logicalMap, builder);
            return new MapLayer<BlendedSpriteInfo>(logicalMap, builder, behavior);
        }
        else
        {
            var builder = new SimpleMapLayerInfoBuilder(tileset, tileSize);
            var behavior = CreateMapLayerBehavior(type, logicalMap, builder);
            return new MapLayer<SpriteInfo>(logicalMap, builder, behavior);
        }
    }


    static IMapLayerBehavior<TInstance> CreateMapLayerBehavior<TInstance>(
        MapLayerType type,
        LogicalMap2D logicalMap,
        IMapLayerInfoBuilder<TInstance> builder) => 
        type switch
        {
            MapLayerType.Underlay => new UnderlayMapLayerBehavior<TInstance>(logicalMap, builder),
            MapLayerType.Overlay => new OverlayMapLayerBehavior<TInstance>(logicalMap, builder),
            _ => throw new NotSupportedException(
                $"MapLayerType {type} is not currently handled by {nameof(VeldridGameFactory)}")
        };
    //*/
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