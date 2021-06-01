using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Entities.Map2D;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities.Map3D
{
    public class MapRenderable3D : Component
    {
        readonly MapId _mapId;
        readonly LogicalMap3D _logicalMap;
        readonly LabyrinthData _labyrinthData;
        readonly DungeonTileMapProperties _properties;
        readonly IDictionary<int, IList<int>> _tilesByDistance = new Dictionary<int, IList<int>>();
        readonly ISet<int> _dirty = new HashSet<int>();
        DungeonTilemap _tilemap;
        bool _isSorting;
        bool _fullUpdate = true;
        int _frameCount;

        public MapRenderable3D(MapId mapId, LogicalMap3D logicalMap, LabyrinthData labyrinthData, in DungeonTileMapProperties properties)
        {
            if (logicalMap == null) throw new ArgumentNullException(nameof(logicalMap));
            if (labyrinthData == null) throw new ArgumentNullException(nameof(labyrinthData));

            On<SlowClockEvent>(OnSlowClock);
            On<RenderEvent>(_ => Update(false));
            On<SortMapTilesEvent>(e => _isSorting = e.IsSorting);
            On<LoadPaletteEvent>(e => { });

            _mapId = mapId;
            _logicalMap = logicalMap;
            _labyrinthData = labyrinthData;
            _properties = properties;

            _logicalMap.Dirty += (sender, args) =>
            {
                if (args.Type == IconChangeType.Floor || args.Type == IconChangeType.Ceiling || args.Type == IconChangeType.Wall)
                    _dirty.Add(_logicalMap.Index(args.X, args.Y));
            };
        }

        protected override void Subscribed()
        {
            Raise(new LoadPaletteEvent(_logicalMap.PaletteId));

            if (_tilemap != null)
                return;

            var assets = Resolve<IAssetManager>();
            var dayPalette = assets.LoadPalette(_logicalMap.PaletteId);
            IPalette nightPalette = null; // TODO

            _tilemap = new DungeonTilemap(
                _mapId,
                _mapId.ToString(),
                _logicalMap.Width * _logicalMap.Height,
                _properties,
                Resolve<ICoreFactory>(),
                dayPalette,
                nightPalette);

            for (int i = 0; i < _labyrinthData.FloorAndCeilings.Count; i++)
            {
                var floorInfo = _labyrinthData.FloorAndCeilings[i];
                _tilemap.DefineFloor(i + 1, assets.LoadTexture(floorInfo?.SpriteId ?? AssetId.None));
            }

            for (int i = 0; i < _labyrinthData.Walls.Count; i++)
            {
                var wallInfo = _labyrinthData.Walls[i];
                if (wallInfo == null)
                    continue;

                ITexture wall = assets.LoadTexture(wallInfo.SpriteId);
                if (wall == null)
                    continue;

                bool isAlphaTested = (wallInfo.Properties & Wall.WallFlags.AlphaTested) != 0;
                _tilemap.DefineWall(i + 1, wall, 0, 0, wallInfo.TransparentColour, isAlphaTested);

                foreach(var overlayInfo in wallInfo.Overlays)
                {
                    if (overlayInfo.SpriteId.IsNone)
                        continue;

                    var overlay = assets.LoadTexture(overlayInfo.SpriteId);
                    _tilemap.DefineWall(i + 1, overlay, overlayInfo.XOffset, overlayInfo.YOffset, wallInfo.TransparentColour, isAlphaTested);
                }
            }

            Resolve<IEngine>()?.RegisterRenderable(_tilemap);
            _fullUpdate = true;
        }

        protected override void Unsubscribed()
        {
            Resolve<IEngine>()?.UnregisterRenderable(_tilemap);
            _tilemap = null;
        }

        void SetTile(int index, int order, int frameCount)
        {
            var (floorIndex, floor) = _logicalMap.GetFloor(index);
            var (ceilingIndex, ceiling) = _logicalMap.GetCeiling(index);
            var (wallIndex, wall) = _logicalMap.GetWall(index);

            bool floorFlag = ((floor?.Properties ?? 0) & FloorAndCeiling.FcFlags.BackAndForth) > 0;
            bool ceilingFlag = ((ceiling?.Properties ?? 0) & FloorAndCeiling.FcFlags.BackAndForth) > 0;
            bool wallFlag = ((wall?.Properties ?? 0) & Wall.WallFlags.BackAndForth) > 0;

            Tile3DFlags flags =
                (floorFlag ? Tile3DFlags.FloorBackAndForth : 0) |
                (ceilingFlag ? Tile3DFlags.CeilingBackAndForth : 0) |
                (wallFlag ? Tile3DFlags.WallBackAndForth : 0);

            _tilemap.Set(order, floorIndex, ceilingIndex, wallIndex, frameCount, flags);
        }

        void OnSlowClock(SlowClockEvent e)
        {
            _frameCount += e.Delta;
            if (_isSorting)
                SortingUpdate();
            else
                Update(true);
        }

        void Update(bool frameChanged)
        {
            if (frameChanged)
                _dirty.UnionWith(_tilemap.AnimatedTiles);

            if (_fullUpdate)
            {
                using var _ = PerfTracker.FrameEvent("5.1 Update tilemap");
                for (int j = 0; j < _logicalMap.Height; j++)
                {
                    for (int i = 0; i < _logicalMap.Width; i++)
                    {
                        int index = j * _logicalMap.Width + i;
                        SetTile(index, index, _frameCount);
                    }
                }

                _fullUpdate = false;
            }
            else if (_dirty.Count > 0)
            {
                foreach (var index in _dirty)
                    SetTile(index, index, _frameCount);
            }
            _dirty.Clear();
        }

        void SortingUpdate()
        {
            using var _ = PerfTracker.FrameEvent("5.1 Update tilemap (sorting)");

            foreach (var list in _tilesByDistance.Values)
                list.Clear();

            var cameraTilePosition = Resolve<ICamera>().Position;

            var map = Resolve<IMapManager>().Current;
            if (map != null)
                cameraTilePosition /= map.TileSize;

            int cameraTileX = (int)cameraTilePosition.X;
            int cameraTileY = (int)cameraTilePosition.Y;

            for (int j = 0; j < _logicalMap.Height; j++)
            {
                for (int i = 0; i < _logicalMap.Width; i++)
                {
                    int distance = Math.Abs(j - cameraTileY) + Math.Abs(i - cameraTileX);
                    if(!_tilesByDistance.TryGetValue(distance, out var list))
                    {
                        list = new List<int>();
                        _tilesByDistance[distance] = list;
                    }

                    int index = j * _logicalMap.Width + i;
                    list.Add(index);
                }
            }

            int order = 0;
            foreach (var distance in _tilesByDistance.OrderByDescending(x => x.Key).ToList())
            {
                if (distance.Value.Count == 0)
                {
                    _tilesByDistance.Remove(distance.Key);
                    continue;
                }

                foreach (var index in distance.Value)
                {
                    SetTile(index, order, _frameCount);
                    order++;
                }
            }
        }
    }
}
