using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Game.Entities.Map2D;

namespace UAlbion.Game.Veldrid.Visual;

public sealed class TileRenderableManager : ServiceComponent<ITileRenderableManager>, ITileRenderableManager
{
    public void Collect(List<IRenderable> renderables)
    {
        if (renderables == null) throw new ArgumentNullException(nameof(renderables));
        foreach (var child in Children)
            if (child is TileLayerRenderable renderable)
                renderables.Add(renderable);
    }

    public TilesetResourceHolder AcquireTilesetResources(ITileGraphics tileset, LogicalMap2D logicalMap, Vector2 tileSize)
    {
        if (tileset == null) throw new ArgumentNullException(nameof(tileset));
        if (logicalMap == null) throw new ArgumentNullException(nameof(logicalMap));

        foreach(var child in Children)
        {
            if (child is TilesetResourceHolder existing && existing.Texture == tileset.Texture)
            {
                existing.RefCount++;
                return existing;
            }
        }

        var gpuTiles = new GpuTileData[logicalMap.TileData.Tiles.Count];
        for (int i = 0; i < gpuTiles.Length; i++)
        {
            var tile = logicalMap.TileData.Tiles[i];
            gpuTiles[i] = new GpuTileData
            {
                Layer = (byte)tile.Layer,
                Type = (byte)tile.Type,
                DayImage = tileset.GetDayRegionId(tile.ImageNumber),
                NightImage = tileset.GetNightRegionId(tile.ImageNumber),
                FrameCount = tile.FrameCount,
                Unk7 = tile.Unk7,
                Flags = (GpuTileFlags)(uint)tile.RawFlags,
                PalFrames = tileset.GetPaletteFrameCount(tile.ImageNumber)
            };
        }

        var renderable = 
            tileset is TrueColorTileGraphics
            ? new TilesetResourceHolder(tileSize, gpuTiles, tileset.Texture, SpriteSampler.TriLinear, true)
            : new TilesetResourceHolder(tileSize, gpuTiles, tileset.Texture, SpriteSampler.Point, false);

        renderable.RefCount = 1;
        AttachChild(renderable);
        return renderable;
    }

    public TileLayerRenderable CreateTileRenderable(string name, byte width, ReadOnlySpan<uint> map, DrawLayer renderOrder, TilesetResourceHolder tileset)
    {
        if (tileset == null) throw new ArgumentNullException(nameof(tileset));
        var renderable = new TileLayerRenderable(name, width, map, renderOrder, tileset);
        AttachChild(renderable);
        return renderable;
    }

    public void ReleaseTilesetResources(TilesetResourceHolder resourceHolder)
    {
        if (resourceHolder == null) throw new ArgumentNullException(nameof(resourceHolder));
        resourceHolder.RefCount--;
        if (resourceHolder.RefCount == 0)
        {
            RemoveChild(resourceHolder);
            resourceHolder.Dispose();
        }
    }

    public void DisposeTileRenderable(TileLayerRenderable layerRenderable)
    {
        if (layerRenderable == null) throw new ArgumentNullException(nameof(layerRenderable));
        RemoveChild(layerRenderable);
        layerRenderable.Dispose();
    }

    public void Dispose()
    {
        foreach (var child in Children.ToList())
            if (child is IDisposable disposable)
                disposable.Dispose();

        RemoveAllChildren();
    }
}