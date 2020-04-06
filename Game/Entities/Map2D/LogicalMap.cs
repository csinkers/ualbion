using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Map;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Entities.Map2D
{
    public class LogicalMap
    {
        readonly MapData2D _mapData;
        readonly TilesetData _tileData;
        readonly IList<Block> _blockList;
        readonly IDictionary<int, MapDelta> _changes;

        public LogicalMap(IAssetManager assetManager, MapDataId mapId, MapData2D mapData)
        {
            _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
            _tileData = assetManager.LoadTileData(_mapData.TilesetId);
            _blockList = assetManager.LoadBlockList((BlockListId)_mapData.TilesetId); // Note: Assuming a 1:1 correspondence between blocklist and tileset ids.
            UseSmallSprites = _tileData.UseSmallGraphics;
        }

        public event EventHandler<DirtyTileEventArgs> Dirty;
        public int Width => _mapData.Width;
        public int Height => _mapData.Height;
        public bool UseSmallSprites { get; }
        public PaletteId PaletteId => _mapData.PaletteId;
        public IconGraphicsId TilesetId => (IconGraphicsId)_mapData.TilesetId;
        public IEnumerable<MapNpc> Npcs => _mapData.Npcs.OrderBy(x => x.Key).Select(x => x.Value);
        public Vector2 TileSize { get; set; } // TODO: Tidy up how this gets initialised

        public int Index(int x, int y) => y * _mapData.Width + x;

        public TileData GetUnderlay(int x, int y) => GetUnderlay(Index(x, y));
        public TileData GetUnderlay(int index)
        {
            if (index < 0 || index >= _mapData.Underlay.Length)
                return null;

            int tileIndex = _mapData.Underlay[index];
            return tileIndex > 1 ? _tileData.Tiles[tileIndex] : null;
        }

        public TileData GetOverlay(int x, int y) => GetOverlay(Index(x, y));
        public TileData GetOverlay(int index)
        {
            if (index < 0 || index >= _mapData.Overlay.Length)
                return null;

            int tileIndex = _mapData.Overlay[index];
            return tileIndex > 1 ? _tileData.Tiles[tileIndex] : null;
        }

        public MapEventZone GetZone(int x, int y) => GetZone(Index(x, y));
        public MapEventZone GetZone(int index) => _mapData.ZoneLookup.TryGetValue(index, out var zone) ? zone : null;

        public IEnumerable<MapEventZone> GetZonesOfType(TriggerType triggerType)
        {
            var matchingKeys = _mapData.ZoneTypeLookup.Keys.Where(x => (x & triggerType) == triggerType);
            return matchingKeys.SelectMany(x => _mapData.ZoneTypeLookup[x]);
        }

        public void ChangeUnderlay(byte x, byte y, ushort value)
        {
            var index = Index(x, y);
            if (index < 0 || index >= _mapData.Underlay.Length)
            {
                CoreUtil.LogError($"Tried to update invalid underlay index {index} (max {_mapData.Underlay.Length}");
            }
            else
            {
                _mapData.Underlay[Index(x, y)] = value;
                Dirty?.Invoke(this, new DirtyTileEventArgs(x, y, IconChangeType.Underlay));
            }
        }

        public void ChangeOverlay(byte x, byte y, ushort value)
        {
            var index = Index(x, y);
            if (index < 0 || index >= _mapData.Overlay.Length)
            {
                CoreUtil.LogError($"Tried to update invalid overlay index {index} (max {_mapData.Overlay.Length}");
            }
            else
            {
                _mapData.Overlay[Index(x, y)] = value;
                Dirty?.Invoke(this, new DirtyTileEventArgs(x, y, IconChangeType.Overlay));
            }
        }

        public void PlaceBlock(byte x, byte y, ushort blockId, bool overwrite)
        {
            if (blockId >= _blockList.Count)
            {
                CoreUtil.LogError($"Tried to set out-of-range block {blockId} (max id {_blockList.Count-1})");
                return;
            }

            var block = _blockList[blockId];
            for (int j = 0; j < block.Height; j++)
            {
                for (int i = 0; i < block.Width; i++)
                {
                    var targetIndex = Index(x + i, y + j);
                    var targetBlockIndex = j * block.Width + i;

                    if(targetIndex < 0 || targetIndex > _mapData.Underlay.Length)
                    {
                        CoreUtil.LogError($"Tried to set out-of-range index {targetIndex}, @ ({x},{y}) + ({i},{j}) for block {blockId}");
                        return;
                    }

                    int underlay = _mapData.Underlay[targetIndex];
                    int newUnderlay = block.Underlay[targetBlockIndex];
                    if (newUnderlay > 1 && (overwrite || underlay <= 1))
                    {
                        _mapData.Underlay[targetIndex] = newUnderlay;
                        Dirty?.Invoke(this, new DirtyTileEventArgs(x + i, y + j, IconChangeType.Underlay));
                    }

                    int overlay = _mapData.Overlay[targetIndex];
                    int newOverlay = block.Overlay[targetBlockIndex];
                    if (overwrite || overlay > 1)
                    {
                        _mapData.Overlay[targetIndex] = newOverlay;
                        Dirty?.Invoke(this, new DirtyTileEventArgs(x + i, y + j, IconChangeType.Overlay));
                    }
                }
            }
        }

        public void ChangeTileEventChain(byte x, byte y, ushort value)
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
                zone.Chain = _mapData.Chains[value];
                zone.Node = zone.Chain.Events.First();
            }
        }

        public void ChangeTileEventTrigger(byte x, byte y, ushort value)
        {
            _mapData.ZoneLookup.TryGetValue(Index(x, y), out var zone);
            if(zone != null)
                zone.Trigger = (TriggerType)value;
        }

        public void DisableChain(byte chainNumber)
        {
            var chain = _mapData.Chains[chainNumber];
            chain.Enabled = false;
        }
    }

    public class DirtyTileEventArgs : EventArgs
    {
        public DirtyTileEventArgs(int x, int y, IconChangeType type)
        {
            X = x;
            Y = y;
            Type = type;
        }

        public int X { get; }
        public int Y { get; }
        public IconChangeType Type { get; }
    }
}
