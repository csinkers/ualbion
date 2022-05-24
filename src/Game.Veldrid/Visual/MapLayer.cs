using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Entities.Map2D;
using UAlbion.Game.Settings;
using UAlbion.Game.State;

namespace UAlbion.Game.Veldrid.Visual;

public class SimpleMapLayer : MapLayer<SpriteInfo>
{
    static readonly Region BlankRegion = new(Vector2.Zero, Vector2.Zero, Vector2.Zero, 0);
    protected override SpriteInfo BlankInstance { get; } = new(0, Vector3.Zero, Vector2.Zero, BlankRegion);
    public SimpleMapLayer(LogicalMap2D logicalMap, ITileGraphics tileset, Vector2 tileSize, bool isOverlay) : base(logicalMap, tileset, tileSize, isOverlay) { }
    protected override SpriteInfo BuildInstance(Vector3 position, ushort imageNumber, int palFrame, SpriteFlags flags)
    {
        var subImage = Tileset.GetRegion(imageNumber, palFrame);
        return new SpriteInfo(SpriteFlags.TopLeft, position, TileSize, subImage);
    }
}

public class BlendedMapLayer : MapLayer<BlendedSpriteInfo>
{
    readonly TrueColorTileGraphics _tileset;
    static readonly Region BlankRegion = new(Vector2.Zero, Vector2.Zero, Vector2.Zero, 0);
    protected override BlendedSpriteInfo BlankInstance { get; } = new(0, Vector3.Zero, Vector2.Zero, BlankRegion, BlankRegion);
    public BlendedMapLayer(LogicalMap2D logicalMap, TrueColorTileGraphics tileset, Vector2 tileSize, bool isOverlay) : base(logicalMap, tileset, tileSize, isOverlay) 
        => _tileset = tileset ?? throw new ArgumentNullException(nameof(tileset));

    protected override BlendedSpriteInfo BuildInstance(Vector3 position, ushort imageNumber, int palFrame, SpriteFlags flags)
    {
        var day = _tileset.GetRegion(imageNumber, palFrame);
        var night = _tileset.GetNightRegion(imageNumber, palFrame);
        return new BlendedSpriteInfo(SpriteFlags.TopLeft, position, TileSize, day, night);
    }
}

public abstract class MapLayer<TInstance> : Component, IMapLayer
    where TInstance : unmanaged
{
    protected abstract TInstance BlankInstance { get;  }

    readonly LogicalMap2D _logicalMap;
    readonly bool _isOverlay;
    readonly DrawLayer _drawLayer;
    readonly ISet<(int, int)> _dirty = new HashSet<(int, int)>();
    readonly ISet<(int, int)> _animated = new HashSet<(int, int)>();

#if DEBUG
    DebugFlags _lastDebugFlags;
#endif
    SpriteLease<TInstance> _lease;
    int _lastFrameCount;
    bool _allDirty = true;

    public int? HighlightIndex { get; set; }
    int? _highlightEvent;

    protected Vector2 TileSize { get; }
    protected ITileGraphics Tileset { get; }
    protected abstract TInstance BuildInstance(Vector3 position, ushort imageNumber, int palFrame, SpriteFlags flags);

    public MapLayer(
        LogicalMap2D logicalMap,
        ITileGraphics tileset,
        Vector2 tileSize,
        bool isOverlay)
    {
        _logicalMap = logicalMap ?? throw new ArgumentNullException(nameof(logicalMap));
        _logicalMap.Dirty += (_, args) =>
        {
            var iconChangeType = _isOverlay ? IconChangeType.Overlay : IconChangeType.Underlay;
            if (args.Type != iconChangeType) 
                return;

            var index = _logicalMap.Index(args.X, args.Y);

            if (IsAnimated(index))
                _animated.Add((args.X, args.Y));
            else
                _animated.Remove((args.X, args.Y));

            _dirty.Add((args.X, args.Y));
        };

        Tileset = tileset ?? throw new ArgumentNullException(nameof(tileset));
        TileSize = tileSize;
        _isOverlay = isOverlay;
        _drawLayer = isOverlay ? DrawLayer.Overlay : DrawLayer.Underlay;

        int index = 0;
        for (int j = 0; j < _logicalMap.Height; j++)
        {
            for (int i = 0; i < _logicalMap.Width; i++)
            {
                if (IsAnimated(index))
                    _animated.Add((i, j));
                index++;
            }
        }

        On<RenderEvent>(_ => Render());
    }

    bool IsAnimated(int index)
    {
        var tile = GetTile(index);
        if (tile == null)
            return false;

        return tile.FrameCount > 1 || Tileset.IsPaletteAnimated(tile.ImageNumber);
    }

    public object GetSpriteData(int x, int y) 
        => _lease?.GetInstance(_logicalMap.Index(x, y));

    protected override void Unsubscribed()
    {
        _lease?.Dispose();
        _lease = null;
    }

    TileData GetTile(int index) => _isOverlay ? _logicalMap.GetOverlay(index) : _logicalMap.GetUnderlay(index);
    TileData GetFallbackTile(int index) => !_isOverlay ? null : _logicalMap.GetUnderlay(index);

    TInstance BuildInstanceData(int index, int i, int j, int tickCount)
    {
        var tile = GetTile(index);

        if (tile == null || tile.NoDraw)
            return BlankInstance;

        var pm = Resolve<IPaletteManager>();
        int frame = AnimUtil.GetFrame(tickCount, tile.FrameCount, tile.Bouncy);
        var fallback = GetFallbackTile(index);
        var layer = tile.Layer;

        if (fallback != null && (int)fallback.Layer > (int)tile.Layer)
            layer = fallback.Layer;

        var position = new Vector3(
            new Vector2(i, j) * TileSize,
            DepthUtil.LayerToDepth(layer.ToDepthOffset(), j));

        var flags = SpriteFlags.TopLeft;
#if DEBUG
        var zone = _logicalMap.GetZone(index);
        int eventNum = zone?.Node?.Id ?? -1;

        flags = flags
             | ((_lastDebugFlags & DebugFlags.HighlightTile) != 0 && HighlightIndex == index ? SpriteFlags.Highlight : 0)
             | ((_lastDebugFlags & DebugFlags.HighlightEventChainZones) != 0 && _highlightEvent == eventNum ? SpriteFlags.GreenTint : 0)
             | ((_lastDebugFlags & DebugFlags.HighlightCollision) != 0 && tile.Collision != Passability.Passable ? SpriteFlags.RedTint : 0)
             | ((_lastDebugFlags & DebugFlags.NoMapTileBoundingBoxes) != 0 ? SpriteFlags.NoBoundingBox : 0)
        //     | ((tile.Flags & TileFlags.TextId) != 0 ? SpriteFlags.RedTint : 0)
        //     | (((int) tile.Type) == 8 ? SpriteFlags.GreenTint : 0)
        //     | (((int) tile.Type) == 12 ? SpriteFlags.BlueTint : 0)
        //     | (((int) tile.Type) == 14 ? SpriteFlags.GreenTint | SpriteFlags.RedTint : 0) //&& tickCount % 2 == 0 ? SpriteFlags.Transparent : 0)
            ;
#endif

        var instance = BuildInstance(position, (ushort)(tile.ImageNumber + frame), pm.Frame, flags);

        return instance;
    }

    void Render()
    {
        var frameCount =  (Resolve<IGameState>()?.TickCount ?? 0) / GetVar(GameVars.Time.FastTicksPerMapTileFrame);
#if DEBUG
        var debug = GetVar(UserVars.Debug.DebugFlags);
        if (_lastDebugFlags != debug)
            _allDirty = true;
        _lastDebugFlags = debug;

        if (frameCount != _lastFrameCount && (
                (debug & DebugFlags.HighlightEventChainZones) != 0
                || (debug & DebugFlags.HighlightTile) != 0))
            _allDirty = true;
#endif

        var sm = Resolve<ISpriteManager<TInstance>>();
        if (_lease == null)
        {
            var flags = SpriteKeyFlags.NoDepthTest | SpriteKeyFlags.ClampEdges;
            if (!_isOverlay)
                flags |= SpriteKeyFlags.ZeroOpaque;

            var sampler = Tileset.Texture is IReadOnlyTexture<byte>
                ? SpriteSampler.Point
                : SpriteSampler.TriLinear;

            var key = new SpriteKey(Tileset.Texture, sampler, _drawLayer, flags);
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

        if (!_allDirty && frameCount != _lastFrameCount)
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