using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities.Map2D;

public class MapRenderable2D : Component
{
    readonly LogicalMap2D _logicalMap;
    readonly IMapLayer _underlay;
    readonly IMapLayer _overlay;
    readonly InfoOverlay _info;
    readonly MapAnnotationLayer _annotations;

    public MapRenderable2D(LogicalMap2D logicalMap, ITileGraphics tileset, IGameFactory factory, Vector2 tileSize)
    {
        if (tileset == null) throw new ArgumentNullException(nameof(tileset));
        if (factory == null) throw new ArgumentNullException(nameof(factory));
        _logicalMap = logicalMap ?? throw new ArgumentNullException(nameof(logicalMap));
        TileSize = tileSize;

        _underlay = AttachChild(factory.CreateMapLayer(logicalMap, tileset, tileSize, false));
        _overlay = AttachChild(factory.CreateMapLayer(logicalMap, tileset, tileSize, true)); 
        _info = AttachChild(new InfoOverlay(logicalMap));

        _annotations = AttachChild(new MapAnnotationLayer(logicalMap, TileSize));

        On<ToggleUnderlayEvent>(e => _underlay.IsActive = !_underlay.IsActive);
        On<ToggleOverlayEvent>(e => _overlay.IsActive = !_overlay.IsActive);
    }

    public Vector2 TileSize { get; }
    public PaletteId Palette => _logicalMap.PaletteId;
    public Vector2 SizePixels => new Vector2(_logicalMap.Width, _logicalMap.Height) * TileSize;
    public object GetUnderlaySpriteData(int x, int y) => _underlay.GetSpriteData(x, y);
    public object GetOverlaySpriteData(int x, int y) => _overlay.GetSpriteData(x, y);

    public void SetHighlightIndex(int? index)
    {
        _underlay.HighlightIndex = index;
        _overlay.HighlightIndex = index;
    }

    protected override void Subscribed() => Raise(new LoadPaletteEvent(Palette));
}