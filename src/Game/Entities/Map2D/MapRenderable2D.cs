using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Config;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities.Map2D;

public class MapRenderable2D : Component
{
    readonly LogicalMap2D _logicalMap;
    readonly IMapLayer _mapLayer;
    // readonly IMapLayer _overlay;
    // readonly InfoOverlay _info;
    // readonly MapAnnotationLayer _annotations;
    int _fastFrames;
    int _mapFrame;

    public MapRenderable2D(LogicalMap2D logicalMap, ITileGraphics tileset, IGameFactory factory, Vector2 tileSize)
    {
        if (tileset == null) throw new ArgumentNullException(nameof(tileset));
        if (factory == null) throw new ArgumentNullException(nameof(factory));
        _logicalMap = logicalMap ?? throw new ArgumentNullException(nameof(logicalMap));
        TileSize = tileSize;

        _mapLayer = AttachChild(factory.CreateMapLayer(logicalMap, tileset, tileSize, DrawLayer.Underlay));
        // _info = AttachChild(new InfoOverlay(logicalMap));
        // _annotations = AttachChild(new MapAnnotationLayer(logicalMap, TileSize));

        On<ToggleUnderlayEvent>(_ => _mapLayer.IsUnderlayActive = !_mapLayer.IsUnderlayActive);
        On<ToggleOverlayEvent>(_ => _mapLayer.IsOverlayActive = !_mapLayer.IsOverlayActive);
        On<FastClockEvent>(e =>
        {
            _fastFrames += e.Frames;
            var newMapFrame = _fastFrames / GetVar(GameVars.Time.FastTicksPerMapTileFrame);
            if (newMapFrame != _mapFrame)
            {
                _mapFrame = newMapFrame;
                _mapLayer.FrameNumber = _mapFrame;
            }
        });
    }

    public Vector2 TileSize { get; }
    public PaletteId Palette => _logicalMap.PaletteId;

    public void SetHighlightIndex(int? index)
    {
        _mapLayer.HighlightIndex = index;
        // _overlay.HighlightIndex = index;
    }

    protected override void Subscribed()
    {
        Raise(new LoadPaletteEvent(Palette));
        _logicalMap.Dirty += OnLogicalMapDirty;
    }

    void OnLogicalMapDirty(object _, DirtyTileEventArgs args)
    {
        if (args.Type is IconChangeType.Underlay or IconChangeType.Overlay)
        {
            var index = _logicalMap.Index(args.X, args.Y);
            _mapLayer.SetTile(index, _logicalMap.RawTiles[index]);
        }
    }

    protected override void Unsubscribed()
    {
        _logicalMap.Dirty -= OnLogicalMapDirty;
    }
}