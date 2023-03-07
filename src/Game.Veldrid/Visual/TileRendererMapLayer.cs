using System;
using System.Numerics;
using System.Runtime.InteropServices;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Game.Entities.Map2D;
using UAlbion.Game.Settings;

namespace UAlbion.Game.Veldrid.Visual;

public class TileRendererMapLayer : Component, IMapLayer
{
    readonly LogicalMap2D _map;
    readonly ITileGraphics _tileset;
    readonly Vector2 _tileSize;
    readonly DrawLayer _renderOrder;
    TileLayerRenderable _layerRenderable;
    TilesetResourceHolder _tilesetResources;
    DebugFlags _debugFlags;

    public TileRendererMapLayer(
        LogicalMap2D map,
        ITileGraphics tileset,
        Vector2 tileSize,
        DrawLayer renderOrder)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
        _tileset = tileset ?? throw new ArgumentNullException(nameof(tileset));
        _tileSize = tileSize;
        _renderOrder = renderOrder;
    }

    public int? HighlightIndex { get; set; }
    public int FrameNumber
    {
        get => _layerRenderable?.FrameNumber ?? 0;
        set
        {
            if (_layerRenderable != null)
                _layerRenderable.FrameNumber = value;
        }
    }

    public bool IsUnderlayActive
    {
        get => _layerRenderable?.IsUnderlayActive ?? true;
        set
        {
            if (_layerRenderable != null)
                _layerRenderable.IsUnderlayActive = value;
        }
    }

    public bool IsOverlayActive
    {
        get => _layerRenderable?.IsOverlayActive ?? true;
        set
        {
            if (_layerRenderable != null)
                _layerRenderable.IsOverlayActive = value;
        }
    }

    public DebugFlags DebugFlags
    {
        get => _debugFlags;
        set
        {
            _debugFlags = value;
            if (_layerRenderable == null) return;
            _layerRenderable.IsCollisionLayerActive = (value & DebugFlags.CollisionLayer) != 0;
            _layerRenderable.IsSitLayerActive = (value & DebugFlags.SitLayer) != 0;
            _layerRenderable.IsMiscLayerActive = (value & DebugFlags.MiscLayer) != 0;
            _layerRenderable.IsZoneLayerActive = (value & DebugFlags.ZoneLayer) != 0;
            _layerRenderable.DrawDebugTiles = (value & DebugFlags.ShowDebugTiles) != 0;
        }
    }

    public void SetTile(int index, MapTile tile) => _layerRenderable?.SetTile(index, tile.Raw);
    protected override void Subscribed()
    {
        var manager = Resolve<ITileRenderableManager>();
        _tilesetResources = manager.AcquireTilesetResources(_tileset, _map, _tileSize);
        _layerRenderable = manager.CreateTileRenderable(
            $"{_map.Id}:{_renderOrder}",
            (byte)_map.Width,
            MemoryMarshal.Cast<MapTile, uint>(_map.RawTiles),
            _renderOrder,
            _tilesetResources);

        DebugFlags = _debugFlags; // Make sure the flags on the new renderable are set
    }

    protected override void Unsubscribed()
    {
        if (_tilesetResources == null)
            return;

        var manager = Resolve<ITileRenderableManager>();
        manager.ReleaseTilesetResources(_tilesetResources);
        manager.DisposeTileRenderable(_layerRenderable);
        _tilesetResources = null;
        _layerRenderable = null;
    }
}