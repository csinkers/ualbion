using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Game.Entities.Map2D;

namespace UAlbion.Game.Veldrid.Visual;

public interface ITileRenderableManager : IRenderableSource
{
    TilesetResourceHolder AcquireTilesetResources(ITileGraphics tileset, LogicalMap2D logicalMap, Vector2 tileSize);
    void ReleaseTilesetResources(TilesetResourceHolder resourceHolder);
    TileLayerRenderable CreateTileRenderable(string name, byte width, ReadOnlySpan<uint> map, DrawLayer renderOrder, TilesetResourceHolder tileset);
    void DisposeTileRenderable(TileLayerRenderable layerRenderable);
}