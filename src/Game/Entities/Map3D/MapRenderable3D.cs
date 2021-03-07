using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities.Map3D
{
    public class MapRenderable3D : Component
    {
        readonly MapId _mapId;
        readonly MapData3D _mapData;
        readonly LabyrinthData _labyrinthData;
        readonly Vector3 _tileSize;
        readonly IDictionary<int, IList<int>> _tilesByDistance = new Dictionary<int, IList<int>>();
        DungeonTileMap _tilemap;
        bool _isSorting;
        bool _fullUpdate = true;

        public MapRenderable3D(MapId mapId, MapData3D mapData, LabyrinthData labyrinthData, Vector3 tileSize)
        {
            On<SlowClockEvent>(OnSlowClock);
            On<SortMapTilesEvent>(e => _isSorting = e.IsSorting);
            On<LoadPaletteEvent>(e => { });

            _mapId = mapId;
            _mapData = mapData;
            _labyrinthData = labyrinthData;
            _tileSize = tileSize;
        }

        protected override void Subscribed()
        {
            Raise(new LoadPaletteEvent(_mapData.PaletteId));

            if (_tilemap != null)
                return;

            var assets = Resolve<IAssetManager>();
            _tilemap = new DungeonTileMap(_mapId, 
                _mapId.ToString(),
                DrawLayer.Background,
                _tileSize,
                _mapData.Width, _mapData.Height,
                Resolve<ICoreFactory>(),
                Resolve<IPaletteManager>());

            for(int i = 0; i < _labyrinthData.FloorAndCeilings.Count; i++)
            {
                var floorInfo = _labyrinthData.FloorAndCeilings[i];
                ITexture floor = assets.LoadTexture(floorInfo?.SpriteId ?? AssetId.None);
                _tilemap.DefineFloor(i + 1, floor);
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
            byte i = (byte)(index % _mapData.Width);
            byte j = (byte)(index / _mapData.Width);
            byte floorIndex = _mapData.Floors[index];
            byte ceilingIndex = _mapData.Ceilings[index];
            int contents = _mapData.Contents[index];
            byte wallIndex = (byte)(contents < 100 || contents - 100 >= _labyrinthData.Walls.Count
                ? 0
                : contents - 100);

            _tilemap.Set(order, i, j, floorIndex, ceilingIndex, wallIndex, frameCount);
        }

        void OnSlowClock(SlowClockEvent e)
        {
            if (_isSorting)
            {
                SortingUpdate(e);
                return;
            }

            using var _ = PerfTracker.FrameEvent("5.1 Update tilemap");

            if (_fullUpdate)
            {
                for (int j = 0; j < _mapData.Height; j++)
                {
                    for (int i = 0; i < _mapData.Width; i++)
                    {
                        int index = j * _mapData.Width + i;
                        SetTile(index, index, e.FrameCount);
                    }
                }

                _fullUpdate = false;
            }
            else
                foreach (var index in _tilemap.AnimatedTiles)
                    SetTile(index, index, e.FrameCount);
        }

        void SortingUpdate(SlowClockEvent e)
        {
            using var _ = PerfTracker.FrameEvent("5.1 Update tilemap (sorting)");

            foreach (var list in _tilesByDistance.Values)
                list.Clear();

            var scene = Resolve<ISceneManager>().ActiveScene;
            var cameraTilePosition = scene.Camera.Position;

            var map = Resolve<IMapManager>().Current;
            if (map != null)
                cameraTilePosition /= map.TileSize;

            int cameraTileX = (int)cameraTilePosition.X;
            int cameraTileY = (int)cameraTilePosition.Y;

            for (int j = 0; j < _mapData.Height; j++)
            {
                for (int i = 0; i < _mapData.Width; i++)
                {
                    int distance = Math.Abs(j - cameraTileY) + Math.Abs(i - cameraTileX);
                    if(!_tilesByDistance.TryGetValue(distance, out var list))
                    {
                        list = new List<int>();
                        _tilesByDistance[distance] = list;
                    }

                    int index = j * _mapData.Width + i;
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
                    SetTile(index, order, e.FrameCount);
                    order++;
                }
            }
        }
    }
}
