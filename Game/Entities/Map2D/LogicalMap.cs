using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Entities.Map2D
{
    public class LogicalMap
    {
        readonly MapData2D _mapData;
        readonly TilesetData _tileData;
        readonly IList<Block> _blockList;

        public LogicalMap(IAssetManager assetManager, MapDataId mapId)
        {
            _mapData = assetManager.LoadMap2D(mapId);
            _tileData = assetManager.LoadTileData((IconDataId)_mapData.TilesetId);
            _blockList = assetManager.LoadBlockList((BlockListId) _mapData.TilesetId);
            UseSmallSprites = _tileData.UseSmallGraphics;
        }

        public event EventHandler<EventArgs> Dirty;
        public int Width => _mapData.Width;
        public int Height => _mapData.Height;
        public bool UseSmallSprites { get; }
        public PaletteId PaletteId => (PaletteId)_mapData.PaletteId;
        public IconGraphicsId TilesetId => (IconGraphicsId)_mapData.TilesetId;
        public IEnumerable<MapNpc> Npcs => _mapData.Npcs;
        public Vector2 TileSize { get; set; } // TODO: Tidy up

        public int Index(int x, int y) => y * _mapData.Width + x;

        public TilesetData.TileData GetUnderlay(int x, int y) => GetUnderlay(Index(x, y));
        public TilesetData.TileData GetUnderlay(int index)
        {
            if (index < 0 || index >= _mapData.Underlay.Length)
                return null;

            int tileIndex = _mapData.Underlay[index];
            return tileIndex != -1 ? _tileData.Tiles[tileIndex] : null;
        }

        public TilesetData.TileData GetOverlay(int x, int y) => GetOverlay(Index(x, y));
        public TilesetData.TileData GetOverlay(int index)
        {
            if (index < 0 || index >= _mapData.Overlay.Length)
                return null;

            int tileIndex = _mapData.Overlay[index];
            return tileIndex != -1 ? _tileData.Tiles[tileIndex] : null;
        }

        public IEnumerable<MapEventZone> GetZones(int x, int y) => GetZones(Index(x, y));
        public IEnumerable<MapEventZone> GetZones(int index) => _mapData.ZoneLookup.TryGetValue(index, out var zones) ? zones : Enumerable.Empty<MapEventZone>();

        public IEnumerable<MapEventZone> GetGlobalZonesOfType(TriggerType triggerType)
        {
            var matchingKeys = _mapData.ZoneTypeLookup.Keys.Where(x => (x & triggerType) == triggerType);
            return matchingKeys.SelectMany(x => _mapData.ZoneTypeLookup[x]);
        }

        public void ChangeUnderlay(sbyte x, sbyte y, ushort value)
        {
            _mapData.Underlay[Index(x, y)] = value;
            Dirty?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeOverlay(sbyte x, sbyte y, ushort value)
        {
            _mapData.Overlay[Index(x, y)] = value;
            Dirty?.Invoke(this, EventArgs.Empty);
        }

        public void PlaceBlock(sbyte x, sbyte y, ushort blockId, bool overwrite)
        {
            if (blockId >= _blockList.Count)
                return; // TODO: Raise log event

            var block = _blockList[blockId];
            for (int j = 0; j < block.Height; j++)
            {
                for (int i = 0; i < block.Width; i++)
                {
                    var targetIndex = Index(x + i, y + j);
                    var targetBlockIndex = j * block.Width + i;

                    int underlay = _mapData.Underlay[targetIndex];
                    int newUnderlay = block.Underlay[targetBlockIndex];
                    if (newUnderlay != -1 && (overwrite || underlay == -1))
                        _mapData.Underlay[targetIndex] = newUnderlay;

                    int overlay = _mapData.Overlay[targetIndex];
                    int newOverlay = block.Overlay[targetBlockIndex];
                    if (overwrite || overlay == -1)
                        _mapData.Overlay[targetIndex] = newOverlay;
                }
            }

            Dirty?.Invoke(this, EventArgs.Empty);
        }

    }
}