using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Settings;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities.Map2D
{
    public class TileLayer : Component
    {
        static readonly SpriteInstanceData BlankInstance = SpriteInstanceData.TopMid(
            Vector3.Zero, Vector2.Zero,
            new Region(Vector2.Zero, Vector2.Zero, Vector2.Zero, 0),
            0);

        readonly LogicalMap2D _logicalMap;
        readonly ITexture _tileset;
        readonly Func<int, TileData> _tileFunc;
        readonly DrawLayer _drawLayer;
        readonly ISet<(int, int)> _dirty = new HashSet<(int, int)>();

#if DEBUG
        DebugFlags _lastDebugFlags;
#endif
        SpriteLease _lease;
        (int, int)[] _animatedTiles;
        int _lastFrameCount;
        bool _allDirty = true;

        public int? HighlightIndex { get; set; }
        int? _highlightEvent;

        public TileLayer(
            LogicalMap2D logicalMap,
            ITexture tileset,
            Func<int, TileData> tileFunc,
            DrawLayer drawLayer,
            IconChangeType iconChangeType)
        {
            _logicalMap = logicalMap ?? throw new ArgumentNullException(nameof(logicalMap));
            _logicalMap.Dirty += (_, args) =>
            {
                if (args.Type == iconChangeType)
                    _dirty.Add((args.X, args.Y));
            };
            _tileset = tileset;
            _tileFunc = tileFunc;
            _drawLayer = drawLayer;

            On<RenderEvent>(_ => Render());
        }

        public WeakSpriteReference GetWeakSpriteReference(int x, int y)
        {
            var sm = Resolve<ISpriteManager>();
            return sm.MakeWeakReference(_lease, _logicalMap.Index(x, y));
        }

        protected override void Unsubscribed()
        {
            _lease?.Dispose();
            _lease = null;
        }

        SpriteInstanceData BuildInstanceData(int i, int j, TileData tile, int tickCount)
        {
            if (tile == null || (tile.Flags & TileFlags.Debug) != 0)
                return BlankInstance;

            int index = _logicalMap.Index(i, j);
            int subImageId = tile.GetSubImageForTile(tickCount);
            var subImage = _tileset.Regions[subImageId];

            var position = new Vector3(
                new Vector2(i, j) * subImage.Size,
                DepthUtil.LayerToDepth(tile.Depth, j));

            var instance = SpriteInstanceData.TopLeft(position, subImage.Size, subImage, 0);

            var zone = _logicalMap.GetZone(index);
            int eventNum = zone?.Node?.Id ?? -1;

            instance.Flags = 0
#if DEBUG
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
            var config = Resolve<GameConfig>();
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
                var key = new SpriteKey(_tileset, _drawLayer, 0);
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
                _dirty.UnionWith(_animatedTiles);

            _lastFrameCount = frameCount;
            if (_allDirty)
            {
                _lease.Access(static (instances, s) =>
                {
                    var animatedTiles = new List<(int, int)>();

                    int index = 0;
                    for (int j = 0; j < s._logicalMap.Height; j++)
                    {
                        for (int i = 0; i < s._logicalMap.Width; i++)
                        {
                            var tile = s._tileFunc(index);
                            instances[index] = s.BuildInstanceData(i, j, tile, s._lastFrameCount);
                            if (tile?.FrameCount > 1)
                                animatedTiles.Add((i, j));

                            index++;
                        }
                    }
                    s._animatedTiles = animatedTiles.ToArray();
                    s._dirty.Clear();
                    s._allDirty = false;
                }, this);
            }
            else if (_dirty.Count > 0)
            {
                _lease.Access(static(instances, s) =>
                {
                    foreach (var (x, y) in s._dirty)
                    {
                        int index = s._logicalMap.Index(x, y);
                        var tile = s._tileFunc(index);
                        instances[index] = s.BuildInstanceData(
                            x,
                            y,
                            tile,
                            s._lastFrameCount);
                    }

                    s._dirty.Clear();
                }, this);
            }
        }
    }
}
