using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Settings;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities.Map2D
{
    public class TileLayer : Component
    {
        const int TicksPerFrame = 10;
        static readonly SpriteInstanceData BlankInstance = SpriteInstanceData.TopMid(
            Vector3.Zero, Vector2.Zero, 
            Vector2.Zero, Vector2.Zero, 0, 0);

        static readonly HandlerSet Handlers = new HandlerSet(
            H<TileLayer, UpdateEvent>((x, e) => x.Update(false)),
            H<TileLayer, ExchangeDisabledEvent>((x, _) =>
            {
                x._lease?.Dispose();
                x._lease = null;
            })
        );

        public TileLayer(LogicalMap logicalMap, ITexture tileset, Func<int, TilesetData.TileData> tileFunc, DrawLayer drawLayer) : base(Handlers)
        {
            _logicalMap = logicalMap;
            _tileset = tileset;
            _tileFunc = tileFunc;
            _drawLayer = drawLayer;
        }

        readonly LogicalMap _logicalMap;
        readonly ITexture _tileset;
        readonly Func<int, TilesetData.TileData> _tileFunc;
        readonly DrawLayer _drawLayer;

#if DEBUG
        DebugFlags _lastDebugFlags;
#endif
        SpriteLease _lease;
        (int, int)[] _animatedTiles;
        int _lastFrameCount;
        bool _isActive = true;

        public int? HighlightIndex { get; set; }
        int? _highlightEvent;

        public override void Subscribed()
        {
            Update(true);
            base.Subscribed();
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value)
                    return;

                _isActive = value;
                if (!value)
                {
                    _lease?.Dispose();
                    _lease = null;
                }
            }
        }

        SpriteInstanceData BuildInstanceData(int i, int j, TilesetData.TileData tile, int tickCount)
        {
            if (tile == null || tile.Flags.HasFlag(TilesetData.TileFlags.Debug))
                return BlankInstance;

            int index = _logicalMap.Index(i, j);
            int subImage = tile.GetSubImageForTile(tickCount);

            _tileset.GetSubImageDetails(
                subImage,
                out var tileSize,
                out var texPosition,
                out var texSize,
                out var layer);

            DrawLayer drawLayer = tile.Layer.ToDrawLayer();
            var position = new Vector3(
                new Vector2(i, j) * tileSize,
                drawLayer.ToZCoordinate(j));

            var instance = SpriteInstanceData.TopLeft(
                    position,
                    tileSize,
                    texPosition,
                    texSize,
                    layer,
                    0);

            var zone = _logicalMap.GetZone(index);
            int eventNum = zone?.EventNumber ?? -1;

            instance.Flags = 0
#if DEBUG
                | (_lastDebugFlags.HasFlag(DebugFlags.HighlightTile) && HighlightIndex == index ? SpriteFlags.Highlight : 0)
                | (_lastDebugFlags.HasFlag(DebugFlags.HighlightEventChainZones) && _highlightEvent == eventNum ? SpriteFlags.GreenTint : 0)
                | (_lastDebugFlags.HasFlag(DebugFlags.HighlightCollision) && tile.Collision != TilesetData.Passability.Passable ? SpriteFlags.RedTint : 0)
                | (_lastDebugFlags.HasFlag(DebugFlags.NoMapTileBoundingBoxes) ? SpriteFlags.NoBoundingBox : 0)
#endif
                // | ((tile.Flags & TilesetData.TileFlags.TextId) != 0 ? SpriteFlags.RedTint : 0)
                // | (((int) tile.Type) == 8 ? SpriteFlags.GreenTint : 0)
                // | (((int) tile.Type) == 12 ? SpriteFlags.BlueTint : 0)
                // | (((int) tile.Type) == 14 ? SpriteFlags.GreenTint | SpriteFlags.RedTint : 0) //&& tickCount % 2 == 0 ? SpriteFlags.Transparent : 0)
                ;

            return instance;
        }

        void Update(bool updateAll)
        {
            var frameCount =  (Resolve<IGameState>()?.TickCount ?? 0) / TicksPerFrame;
#if DEBUG
            var debug = Resolve<IDebugSettings>()?.DebugFlags ?? 0;
            if (_lastDebugFlags != debug)
                updateAll = true;
            _lastDebugFlags = debug;

            if (frameCount != _lastFrameCount && (
                    debug.HasFlag(DebugFlags.HighlightEventChainZones)
                 || debug.HasFlag(DebugFlags.HighlightTile)))
                updateAll = true;
#endif

            var sm = Resolve<ISpriteManager>();
            if (_isActive && _lease == null)
            {
                var key = new SpriteKey(_tileset, _drawLayer, 0);
                _lease = sm.Borrow(key, _logicalMap.Width * _logicalMap.Height, this);
                updateAll = true;
            }

            if (_lease == null)
                return;

            if (HighlightIndex.HasValue)
            {
                var zone = _logicalMap.GetZone(HighlightIndex.Value);
                _highlightEvent = zone?.EventNumber ?? -1;
                if (_highlightEvent == -1)
                    _highlightEvent = null;
            }
            else _highlightEvent = null;

            if (updateAll)
            {
                var instances = _lease.Access();
                var animatedTiles = new List<(int, int)>();

                int index = 0;
                for (int j = 0; j < _logicalMap.Height; j++)
                {
                    for (int i = 0; i < _logicalMap.Width; i++)
                    {
                        var tile = _tileFunc(index);
                        instances[index] = BuildInstanceData(i, j, tile, frameCount);
                        if(tile?.FrameCount > 1)
                            animatedTiles.Add((i, j));

                        index++;
                    }
                }

                _animatedTiles = animatedTiles.ToArray();
            }
            else if (frameCount != _lastFrameCount)
            {
                var instances = _lease.Access();
                foreach (var (x,y) in _animatedTiles)
                {
                    int index = _logicalMap.Index(x, y);
                    var tile = _tileFunc(index);
                    instances[index] = BuildInstanceData(
                        x,
                        y,
                        tile,
                        frameCount);
                }
            }

            _lastFrameCount = frameCount;
        }
    }
}