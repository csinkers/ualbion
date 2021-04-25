using System;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Entities.Map2D
{
    public class LogicalMap3D : LogicalMap
    {
        readonly MapData3D _mapData;
        readonly LabyrinthData _labyrinth;

        public LogicalMap3D(MapData3D mapData,
            LabyrinthData labyrinth,
            MapChangeCollection tempChanges,
            MapChangeCollection permChanges) : base(mapData, tempChanges, permChanges)
        {
            _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
            _labyrinth = labyrinth ?? throw new ArgumentNullException(nameof(labyrinth));
        }

        protected override void ChangeFloor(byte x, byte y, ushort value)
        {
            var index = Index(x, y);
            if (index < 0 || index >= _mapData.Floors.Length)
            {
                Error($"Tried to update invalid floor index {index} (max {_mapData.Floors.Length}");
            }
            else
            {
                _mapData.Floors[index] = (byte)value;
                OnDirty(x, y, IconChangeType.Floor);
            }
        }

        protected override void ChangeCeiling(byte x, byte y, ushort value)
        {
            var index = Index(x, y);
            if (index < 0 || index >= _mapData.Floors.Length)
            {
                Error($"Tried to update invalid ceiling index {index} (max {_mapData.Ceilings.Length}");
            }
            else
            {
                _mapData.Ceilings[index] = (byte)value;
                OnDirty(x, y, IconChangeType.Ceiling);
            }
        }

        protected override void ChangeWall(byte x, byte y, ushort value)
        {
            var index = Index(x, y);
            if (index < 0 || index >= _mapData.Contents.Length)
            {
                Error($"Tried to update invalid wall/content index {index} (max {_mapData.Contents.Length}");
            }
            else
            {
                _mapData.Contents[index] = (byte)value;
                OnDirty(x, y, IconChangeType.Wall);
            }
        }

        public (byte, FloorAndCeiling) GetFloor(int x, int y) => GetFloor(Index(x, y));
        public (byte, FloorAndCeiling) GetFloor(int index)
        {
            if (index < 0 || index >= _mapData.Floors.Length)
                return (0, null);

            byte tileIndex = _mapData.Floors[index];
            var tile = tileIndex > 0 && tileIndex < _labyrinth.FloorAndCeilings.Count
                ? _labyrinth.FloorAndCeilings[tileIndex]
                : null;
            return (tileIndex, tile);
        }

        public (byte, FloorAndCeiling) GetCeiling(int x, int y) => GetCeiling(Index(x, y));
        public (byte, FloorAndCeiling) GetCeiling(int index)
        {
            if (index < 0 || index >= _mapData.Ceilings.Length)
                return (0, null);

            byte tileIndex = _mapData.Ceilings[index];
            var tile = tileIndex > 0 && tileIndex < _labyrinth.FloorAndCeilings.Count
                ? _labyrinth.FloorAndCeilings[tileIndex]
                : null;
            return (tileIndex, tile);
        }

        public (byte, Wall) GetWall(int x, int y) => GetWall(Index(x, y));
        public (byte, Wall) GetWall(int index)
        {
            byte tileIndex = _mapData.GetWall(index);
            var tile = tileIndex > 0 && tileIndex < _labyrinth.Walls.Count
                ? _labyrinth.Walls[tileIndex - 1]
                : null;
            return (tileIndex, tile);
        }

        public ObjectGroup GetObject(int x, int y) => GetObject(Index(x, y));
        public ObjectGroup GetObject(int index)
        {
            var contents = _mapData.GetObject(index);
            return contents > 0 && contents <= _labyrinth.ObjectGroups.Count
                ? _labyrinth.ObjectGroups[contents - 1]
                : null;
        }
    }
}