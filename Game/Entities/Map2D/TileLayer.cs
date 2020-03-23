using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Settings;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities.Map2D
{
    public class TileLayer : Component
    {
        const int TicksPerFrame = 10;
        static readonly SpriteInstanceData BlankInstance = SpriteInstanceData.TopMid(
            Vector3.Zero, Vector2.Zero,
            new SubImage(Vector2.Zero, Vector2.Zero, Vector2.Zero, 0),
            0);

        static readonly HandlerSet Handlers = new HandlerSet(
            H<TileLayer, RenderEvent>((x, e) => x.Render()),
            H<TileLayer, ExchangeDisabledEvent>((x, _) =>
            {
                x._lease?.Dispose();
                x._lease = null;
            })
        );

        public TileLayer(LogicalMap logicalMap, ITexture tileset, Func<int, TilesetData.TileData> tileFunc, DrawLayer drawLayer, IconChangeType iconChangeType) : base(Handlers)
        {
            _logicalMap = logicalMap;
            _logicalMap.Dirty += (sender, args) =>
            {
                if (args.Type == iconChangeType)
                    _dirty.Add((args.X, args.Y));
            };
            _tileset = tileset;
            _tileFunc = tileFunc;
            _drawLayer = drawLayer;
        }

        readonly LogicalMap _logicalMap;
        readonly ITexture _tileset;
        readonly Func<int, TilesetData.TileData> _tileFunc;
        readonly DrawLayer _drawLayer;
        readonly ISet<(int, int)> _dirty = new HashSet<(int, int)>();

#if DEBUG
        DebugFlags _lastDebugFlags;
#endif
        SpriteLease _lease;
        (int, int)[] _animatedTiles;
        int _lastFrameCount;
        bool _isActive = true;
        bool _allDirty = true;

        public int? HighlightIndex { get; set; }
        int? _highlightEvent;

        public WeakSpriteReference GetWeakSpriteReference(int x, int y)
        {
            var sm = Resolve<ISpriteManager>();
            return sm.MakeWeakReference(_lease, _logicalMap.Index(x, y));
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
            if (tile == null || (tile.Flags & TilesetData.TileFlags.Debug) != 0)
                return BlankInstance;

            int index = _logicalMap.Index(i, j);
            int subImageId = tile.GetSubImageForTile(tickCount);

            var subImage = _tileset.GetSubImageDetails(subImageId);

            DrawLayer drawLayer = tile.ToDrawLayer();
            var position = new Vector3(
                new Vector2(i, j) * subImage.Size,
                drawLayer.ToZCoordinate(j));

            var instance = SpriteInstanceData.TopLeft(position, subImage.Size, subImage, 0);

            var zone = _logicalMap.GetZone(index);
            int eventNum = zone?.EventNumber ?? -1;

            instance.Flags = 0
#if DEBUG
                | ((_lastDebugFlags & DebugFlags.HighlightTile) != 0 && HighlightIndex == index ? SpriteFlags.Highlight : 0)
                | ((_lastDebugFlags & DebugFlags.HighlightEventChainZones) != 0 && _highlightEvent == eventNum ? SpriteFlags.GreenTint : 0)
                | ((_lastDebugFlags & DebugFlags.HighlightCollision) != 0 && tile.Collision != TilesetData.Passability.Passable ? SpriteFlags.RedTint : 0)
                | ((_lastDebugFlags & DebugFlags.NoMapTileBoundingBoxes) != 0 ? SpriteFlags.NoBoundingBox : 0)
#endif
                // | ((tile.Flags & TilesetData.TileFlags.TextId) != 0 ? SpriteFlags.RedTint : 0)
                // | (((int) tile.Type) == 8 ? SpriteFlags.GreenTint : 0)
                // | (((int) tile.Type) == 12 ? SpriteFlags.BlueTint : 0)
                // | (((int) tile.Type) == 14 ? SpriteFlags.GreenTint | SpriteFlags.RedTint : 0) //&& tickCount % 2 == 0 ? SpriteFlags.Transparent : 0)
                ;

            return instance;
        }

        void Render()
        {
            var frameCount =  (Resolve<IGameState>()?.TickCount ?? 0) / TicksPerFrame;
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
            if (_isActive && _lease == null)
            {
                var key = new SpriteKey(_tileset, _drawLayer, 0);
                _lease = sm.Borrow(key, _logicalMap.Width * _logicalMap.Height, this);
                _allDirty = true;
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

            if(!_allDirty && frameCount != _lastFrameCount)
                _dirty.UnionWith(_animatedTiles);

            if (_allDirty)
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
                _dirty.Clear();
                _allDirty = false;
            }
            else if (_dirty.Count > 0)
            {
                var instances = _lease.Access();
                foreach (var (x,y) in _dirty)
                {
                    int index = _logicalMap.Index(x, y);
                    var tile = _tileFunc(index);
                    instances[index] = BuildInstanceData(
                        x,
                        y,
                        tile,
                        frameCount);
                }
                _dirty.Clear();
            }

            _lastFrameCount = frameCount;
        }
    }
}
