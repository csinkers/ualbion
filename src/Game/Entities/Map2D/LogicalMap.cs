using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Entities.Map2D
{
    public abstract class LogicalMap : Component
    {
        readonly BaseMapData _mapData;
        protected MapChangeCollection TempChanges { get; }
        protected MapChangeCollection PermChanges { get; }

        protected LogicalMap(BaseMapData mapData,
            MapChangeCollection tempChanges,
            MapChangeCollection permChanges)
        {
            _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
            TempChanges = tempChanges ?? throw new ArgumentNullException(nameof(tempChanges));
            PermChanges = permChanges ?? throw new ArgumentNullException(nameof(permChanges));

            // Clear out temp changes for other maps
            for(int i = 0; i < tempChanges.Count;)
            {
                if (TempChanges[i].MapId != mapData.Id)
                    TempChanges.RemoveAt(i);
                else i++;
            }

            // Replay any changes for this map
            foreach (var change in permChanges.Where(x => x.MapId == mapData.Id))
                ApplyChange(change.X, change.Y, change.ChangeType, change.Value);

            foreach (var change in tempChanges)
                ApplyChange(change.X, change.Y, change.ChangeType, change.Value);
        }

        public MapId Id => _mapData.Id;
        public int Index(int x, int y) => y * _mapData.Width + x;
        public event EventHandler<DirtyTileEventArgs> Dirty;
        public int Width => _mapData.Width;
        public int Height => _mapData.Height;
        public PaletteId PaletteId => _mapData.PaletteId;
        public IEnumerable<MapNpc> Npcs => _mapData.Npcs;
        public MapEventZone GetZone(int x, int y) => GetZone(Index(x, y));
        public MapEventZone GetZone(int index) => _mapData.ZoneLookup.TryGetValue(index, out var zone) ? zone : null;


        public void Modify(byte x, byte y, IconChangeType changeType, ushort value, bool isTemporary)
        {
            ApplyChange(x, y, changeType, value);
            var collection = isTemporary ? TempChanges : PermChanges;
            collection.Update(Id, x, y, changeType, value);
        }

        public IEnumerable<MapEventZone> GetZonesOfType(TriggerTypes triggerType)
        {
            var matchingKeys = _mapData.ZoneTypeLookup.Keys.Where(x => (x & triggerType) == triggerType);
            return matchingKeys.SelectMany(x => _mapData.ZoneTypeLookup[x]);
        }

        void ApplyChange(byte x, byte y, IconChangeType changeType, ushort value)
        {
            switch (changeType)
            {
                case IconChangeType.Underlay: ChangeUnderlay(x, y, value); break;
                case IconChangeType.Overlay: ChangeOverlay(x, y, value); break;
                case IconChangeType.Wall: break; // N/A for 2D map
                case IconChangeType.Floor: break; // N/A for 2D map
                case IconChangeType.Ceiling: break; // N/A for 2D map
                case IconChangeType.NpcMovement: break;
                case IconChangeType.NpcSprite: break;
                case IconChangeType.Chain: ChangeTileEventChain(x, y, value); break;
                case IconChangeType.BlockHard: PlaceBlock(x, y, value, true); break;
                case IconChangeType.BlockSoft: PlaceBlock(x, y, value, false); break;
                case IconChangeType.Trigger: ChangeTileEventTrigger(x, y, value); break;
                default: throw new ArgumentOutOfRangeException(nameof(changeType), changeType, $"Unexpected change type \"{changeType}\"");
            }
        }

        protected virtual void ChangeUnderlay(byte x, byte y, ushort value) { } // 2D only
        protected virtual void ChangeOverlay(byte x, byte y, ushort value) { } // 2D only
        protected virtual void PlaceBlock(byte x, byte y, ushort blockId, bool overwrite) { } // 2D only
        protected virtual void ChangeWall(byte x, byte y, ushort value) { } // 3D only
        protected virtual void ChangeFloor(byte x, byte y, ushort value) { } // 3D only
        protected virtual void ChangeCeiling(byte x, byte y, ushort value) { } // 3D only

        protected virtual void ChangeNpcMovement(byte x, byte y, ushort value) { } // TODO
        protected virtual void ChangeNpcSprite(byte x, byte y, ushort value) { } // TODO

        void ChangeTileEventChain(byte x, byte y, ushort value)
        {
            var index = Index(x, y);
            _mapData.ZoneLookup.TryGetValue(index, out var zone);
            if (zone == null)
                return;

            if (value >= _mapData.Chains.Count)
            {
                _mapData.ZoneLookup.Remove(index);
                _mapData.Zones.Remove(zone);
            }
            else
            {
                zone.ChainSource = _mapData.Id;
                zone.Chain = value;
                var firstEventId = _mapData.Chains[value];
                zone.Node = firstEventId >= _mapData.Events.Count ? null : _mapData.Events[firstEventId];
            }
        }

        void ChangeTileEventTrigger(byte x, byte y, ushort value)
        {
            _mapData.ZoneLookup.TryGetValue(Index(x, y), out var zone);
            if(zone != null)
                zone.Trigger = (TriggerTypes)value;
        }

        protected void OnDirty(int x, int y, IconChangeType type) => Dirty?.Invoke(this, new DirtyTileEventArgs(x, y, type));
    }

    public class DirtyTileEventArgs : EventArgs
    {
        public DirtyTileEventArgs(int x, int y, IconChangeType type) { X = x; Y = y; Type = type; }
        public int X { get; }
        public int Y { get; }
        public IconChangeType Type { get; }
    }
}