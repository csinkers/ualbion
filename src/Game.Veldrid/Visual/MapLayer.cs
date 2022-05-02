using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Entities.Map2D;
using UAlbion.Game.Settings;
using UAlbion.Game.State;

namespace UAlbion.Game.Veldrid.Visual;

public class MapLayer : Component, IMapLayer
{
    static readonly Region BlankRegion = new(Vector2.Zero, Vector2.Zero, Vector2.Zero, 0);
    static readonly SpriteInstanceData BlankInstance = new(Vector3.Zero, Vector2.Zero, BlankRegion, 0);

    readonly LogicalMap2D _logicalMap;
    readonly ITileGraphics _tileset;
    readonly bool _isOverlay;
    readonly DrawLayer _drawLayer;
    readonly ISet<(int, int)> _dirty = new HashSet<(int, int)>();
    readonly ISet<(int, int)> _animated = new HashSet<(int, int)>();

#if DEBUG
    DebugFlags _lastDebugFlags;
#endif
    SpriteLease _lease;
    int _lastFrameCount;
    bool _allDirty = true;

    public int? HighlightIndex { get; set; }
    int? _highlightEvent;

    public MapLayer(
        LogicalMap2D logicalMap,
        ITileGraphics tileset,
        bool isOverlay)
    {
        _logicalMap = logicalMap ?? throw new ArgumentNullException(nameof(logicalMap));
        _logicalMap.Dirty += (_, args) =>
        {
            var iconChangeType = _isOverlay ? IconChangeType.Overlay : IconChangeType.Underlay;
            if (args.Type != iconChangeType) 
                return;

            var index = _logicalMap.Index(args.X, args.Y);

            if (GetTile(index)?.FrameCount > 1)
                _animated.Add((args.X, args.Y));
            else
                _animated.Remove((args.X, args.Y));

            _dirty.Add((args.X, args.Y));
        };

        _tileset = tileset;
        _isOverlay = isOverlay;
        _drawLayer = isOverlay ? DrawLayer.Overlay : DrawLayer.Underlay;

        int index = 0;
        for (int j = 0; j < _logicalMap.Height; j++)
        {
            for (int i = 0; i < _logicalMap.Width; i++)
            {
                if (GetTile(index)?.FrameCount > 1)
                    _animated.Add((i, j));
                index++;
            }
        }

        On<RenderEvent>(_ => Render());
    }

    public SpriteInstanceData? GetSpriteData(int x, int y) 
        => _lease?.GetInstance(_logicalMap.Index(x, y));

    protected override void Unsubscribed()
    {
        _lease?.Dispose();
        _lease = null;
    }

    TileData GetTile(int index) => _isOverlay ? _logicalMap.GetOverlay(index) : _logicalMap.GetUnderlay(index);
    TileData GetFallbackTile(int index) => !_isOverlay ? null : _logicalMap.GetUnderlay(index);

    SpriteInstanceData BuildInstanceData(int index, int i, int j, int tickCount)
    {
        var tile = GetTile(index);

        if (tile == null || tile.NoDraw)
            return BlankInstance;

        var pm = Resolve<IPaletteManager>();
        int frame = AnimUtil.GetFrame(tickCount, tile.FrameCount, tile.Bouncy);
        var subImage = _tileset.GetRegion(tile.ImageNumber, frame, pm.Frame);
        var fallback = GetFallbackTile(index);
        var layer = tile.Layer;

        if (fallback != null && (int)fallback.Layer > (int)tile.Layer)
            layer = fallback.Layer;

        var position = new Vector3(
            new Vector2(i, j) * subImage.Size,
            DepthUtil.LayerToDepth(layer.ToDepthOffset(), j));

        var instance = new SpriteInstanceData(position, subImage.Size, subImage, SpriteFlags.TopLeft);

        var zone = _logicalMap.GetZone(index);
        int eventNum = zone?.Node?.Id ?? -1;

#if DEBUG
        instance.Flags = instance.Flags
                         | ((_lastDebugFlags & DebugFlags.HighlightTile) != 0 && HighlightIndex == index ? SpriteFlags.Highlight : 0)
                         | ((_lastDebugFlags & DebugFlags.HighlightEventChainZones) != 0 && _highlightEvent == eventNum ? SpriteFlags.GreenTint : 0)
                         | ((_lastDebugFlags & DebugFlags.HighlightCollision) != 0 && tile.Collision != Passability.Passable ? SpriteFlags.RedTint : 0)
                         | ((_lastDebugFlags & DebugFlags.NoMapTileBoundingBoxes) != 0 ? SpriteFlags.NoBoundingBox : 0)
#endif
            // | ((tile.Flags & TileFlags.TextId) != 0 ? SpriteFlags.RedTint : 0)
            // | (((int) tile.Type) == 8 ? SpriteFlags.GreenTint : 0)
            // | (((int) tile.Type) == 12 ? SpriteFlags.BlueTint : 0)
            // | (((int) tile.Type) == 14 ? SpriteFlags.GreenTint | SpriteFlags.RedTint : 0) //&& tickCount % 2 == 0 ? SpriteFlags.Transparent : 0)
            ;

        return instance;
    }

    void Render()
    {
        var config = Resolve<IGameConfigProvider>().Game;
        var frameCount =  (Resolve<IGameState>()?.TickCount ?? 0) / config.Time.FastTicksPerMapTileFrame;
#if DEBUG
        var debug = Resolve<IDebugSettings>()?.DebugFlags ?? 0;
        if (_lastDebugFlags != debug)
            _allDirty = true;
        _lastDebugFlags = debug;

        if (frameCount != _lastFrameCount && (
                (debug & DebugFlags.HighlightEventChainZones) != 0
                || (debug & DebugFlags.HighlightTile) != 0))
            _allDirty = true;
#endif

        var sm = Resolve<ISpriteManager>();
        if (_lease == null)
        {
            var flags = SpriteKeyFlags.NoDepthTest;
            if (!_isOverlay)
                flags |= SpriteKeyFlags.ZeroOpaque;

            var key = new SpriteKey(_tileset.Texture, SpriteSampler.Point, _drawLayer, flags);
            _lease = sm.Borrow(key, _logicalMap.Width * _logicalMap.Height, this);
            _allDirty = true;
        }

        if (_lease == null)
            return;

        if (HighlightIndex.HasValue)
        {
            var zone = _logicalMap.GetZone(HighlightIndex.Value);
            _highlightEvent = zone?.Node?.Id ?? -1;
            if (_highlightEvent == -1)
                _highlightEvent = null;
        }
        else _highlightEvent = null;

        if(!_allDirty && frameCount != _lastFrameCount)
            _dirty.UnionWith(_animated);

        _lastFrameCount = frameCount;
        if (_allDirty)
        {
            _lease.Access(static (instances, s) =>
            {
                int index = 0;
                for (int j = 0; j < s._logicalMap.Height; j++)
                {
                    for (int i = 0; i < s._logicalMap.Width; i++)
                    {
                        instances[index] = s.BuildInstanceData(index, i, j, s._lastFrameCount);
                        index++;
                    }
                }

                s._dirty.Clear();
                s._allDirty = false;
            }, this);
        }
        else if (_dirty.Count > 0)
        {
            _lease.Access(static (instances, s) =>
            {
                foreach (var (x, y) in s._dirty)
                {
                    int index = s._logicalMap.Index(x, y);
                    instances[index] = s.BuildInstanceData(index, x, y, s._lastFrameCount);
                }

                s._dirty.Clear();
            }, this);
        }
    }
}