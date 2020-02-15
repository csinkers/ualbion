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

namespace UAlbion.Game.Entities
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

        public TileLayer(MapData2D mapData, TilesetData tileData, ITexture tileset, int[] tileIds, DrawLayer drawLayer) : base(Handlers)
        {
            _mapData = mapData;
            _tileData = tileData;
            _tileset = tileset;
            _tileIds = tileIds;
            _drawLayer = drawLayer;
        }

        readonly MapData2D _mapData;
        readonly ITexture _tileset;
        readonly TilesetData _tileData;
        readonly int[] _tileIds;
        readonly DrawLayer _drawLayer;

#if DEBUG
        DebugFlags _lastDebugFlags;
#endif
        SpriteLease _lease;
        int[] _animatedIndices;
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

            int index = j * _mapData.Width + i;
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

            var instance = _tileData.UseSmallGraphics 
                ? SpriteInstanceData.TopMid(
                    position,
                    tileSize,
                    texPosition,
                    texSize,
                    layer,
                    0)
                : SpriteInstanceData.TopLeft(
                    position,
                    tileSize,
                    texPosition,
                    texSize,
                    layer,
                    0);

            int zoneNum = _mapData.ZoneLookup[index];
            int eventNum = zoneNum == -1 ? -1 : _mapData.Zones[zoneNum].EventNumber;

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
#if DEBUG
            var debug = Resolve<IDebugSettings>()?.DebugFlags ?? 0;
            if (_lastDebugFlags != debug)
                updateAll = true;
            _lastDebugFlags = debug;

            if (debug.HasFlag(DebugFlags.HighlightEventChainZones)
             || debug.HasFlag(DebugFlags.HighlightTile)
             || debug.HasFlag(DebugFlags.HighlightSelection))
                updateAll = true;
#endif

            var frameCount =  (Resolve<IGameState>()?.TickCount ?? 0) / TicksPerFrame;
            var sm = Resolve<ISpriteManager>();
            if (_isActive && _lease == null)
            {
                var key = new SpriteKey(_tileset, _drawLayer, 0);
                _lease = sm.Borrow(key, _mapData.Width * _mapData.Height, this);
                updateAll = true;
            }

            if (_lease == null)
                return;

            if (HighlightIndex.HasValue)
            {
                int zoneNum = _mapData.ZoneLookup[HighlightIndex.Value];
                _highlightEvent = zoneNum == -1 ? -1 : _mapData.Zones[zoneNum].EventNumber;
                if (_highlightEvent == -1)
                    _highlightEvent = null;
            }
            else _highlightEvent = null;

            if (updateAll)
            {
                var instances = _lease.Access();
                var animatedTiles = new List<int>();

                int index = 0;
                for (int j = 0; j < _mapData.Height; j++)
                {
                    for (int i = 0; i < _mapData.Width; i++)
                    {
                        int tileId = _tileIds[index];
                        var tile = tileId == -1 ? null : _tileData.Tiles[tileId];
                        instances[index] = BuildInstanceData(i, j, tile, frameCount);
                        if(tile?.FrameCount > 1)
                            animatedTiles.Add(index);

                        index++;
                    }
                }

                _animatedIndices = animatedTiles.ToArray();
            }
            else if (frameCount != _lastFrameCount)
            {
                var instances = _lease.Access();
                foreach (var index in _animatedIndices)
                {
                    var tileId = _tileIds[index];
                    var tile = tileId == -1 ? null : _tileData.Tiles[tileId];
                    instances[index] = BuildInstanceData(
                        index % _mapData.Width,
                        index / _mapData.Width,
                        tile,
                        frameCount);
                }
            }

            _lastFrameCount = frameCount;
        }
    }
}